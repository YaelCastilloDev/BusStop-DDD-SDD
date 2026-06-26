namespace BusStop.UseCases.Routes.GetNearby;

public interface INearbyRoutesQueryService
{
    Task<NearbyRoutesResult> GetNearbyRoutesAsync(double latitude, double longitude, double initialRadiusKm = 0.3, double fallbackRadiusKm = 20.0, CancellationToken cancellationToken = default);
}
