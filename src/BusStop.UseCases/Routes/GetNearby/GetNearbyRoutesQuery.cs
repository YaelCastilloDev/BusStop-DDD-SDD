namespace BusStop.UseCases.Routes.GetNearby;

public sealed record GetNearbyRoutesQuery(double Latitude, double Longitude, double RadiusKm = 10) : IQuery<Result<List<RouteResponse>>>;
