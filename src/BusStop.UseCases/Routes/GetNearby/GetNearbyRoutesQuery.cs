namespace BusStop.UseCases.Routes.GetNearby;

public sealed record GetNearbyRoutesQuery(double Latitude, double Longitude, double InitialRadiusKm = 0.3, double FallbackRadiusKm = 20.0) : IQuery<Result<NearbyRoutesResult>>;
