using BusStop.UseCases.Routes.GetNearby;
using Microsoft.EntityFrameworkCore;

namespace BusStop.Infrastructure.Data.Queries;

public sealed class NearbyRoutesQueryService(AppDbContext dbContext) : INearbyRoutesQueryService
{
    // SRID 4326 represents the WGS 84 spatial reference system (standard GPS coordinates)
    private const int Wgs84Srid = 4326;

    public async Task<NearbyRoutesResult> GetNearbyRoutesAsync(double latitude, double longitude, double initialRadiusKm = 0.3, double fallbackRadiusKm = 20.0, CancellationToken cancellationToken = default)
    {
        var initialRadiusMeters = initialRadiusKm * 1000;
        var fallbackRadiusMeters = fallbackRadiusKm * 1000;

        var routes = await GetRoutesWithinRadiusAsync(latitude, longitude, initialRadiusMeters, cancellationToken);

        if (routes.Any())
        {
            return new NearbyRoutesResult(
                routes,
                IsClosestMatchOnly: false,
                Message: $"Found {routes.Count} route(s) within {initialRadiusMeters} meters.");
        }

        // Fallback to closest route within fallback radius
        var closestRoute = await GetRoutesWithinRadiusAsync(latitude, longitude, fallbackRadiusMeters, cancellationToken, limit: 1);

        if (closestRoute.Any())
        {
            return new NearbyRoutesResult(
                closestRoute,
                IsClosestMatchOnly: true,
                Message: $"No routes found within {initialRadiusMeters} meters. Showing the closest route within {fallbackRadiusKm} km.");
        }

        return new NearbyRoutesResult(
            new List<NearbyRouteDto>(),
            IsClosestMatchOnly: false,
            Message: $"No routes found within {fallbackRadiusKm} km.");
    }

    private async Task<List<NearbyRouteDto>> GetRoutesWithinRadiusAsync(double latitude, double longitude, double radiusMeters, CancellationToken cancellationToken, int? limit = null)
    {
        // We use raw SQL because Stop.Location is a custom ValueObject with a ValueConverter, 
        // which prevents EF Core from translating LINQ spatial methods (like .Distance()) directly.
        var sql = $@"
            SELECT 
                r.""Id"", 
                r.""Name"", 
                r.""CreatedById"", 
                r.""CreatedAt"", 
                r.""DeletedAt"" IS NOT NULL AS ""IsDeleted"",
                MIN(ST_Distance(s.""Location"", ST_SetSRID(ST_MakePoint({{1}}, {{0}}), {Wgs84Srid})::geography)) AS ""DistanceMeters""
            FROM routes r
            JOIN stops s ON r.""Id"" = s.""RouteId""
            WHERE r.""DeletedAt"" IS NULL AND s.""DeletedAt"" IS NULL
              AND ST_DWithin(s.""Location"", ST_SetSRID(ST_MakePoint({{1}}, {{0}}), {Wgs84Srid})::geography, {{2}})
            GROUP BY r.""Id"", r.""Name"", r.""CreatedById"", r.""CreatedAt"", r.""DeletedAt""
            ORDER BY ""DistanceMeters"" ASC";

        if (limit.HasValue)
        {
            sql += $"\n            LIMIT {limit.Value}";
        }

        return await dbContext.Database.SqlQueryRaw<NearbyRouteDto>(
            sql, 
            latitude, 
            longitude, 
            radiusMeters)
            .ToListAsync(cancellationToken);
    }
}
