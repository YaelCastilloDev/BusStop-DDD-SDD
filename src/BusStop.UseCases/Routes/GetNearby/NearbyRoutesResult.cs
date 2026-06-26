namespace BusStop.UseCases.Routes.GetNearby;

public sealed record NearbyRoutesResult(
    List<NearbyRouteDto> Routes,
    bool IsClosestMatchOnly,
    string Message);