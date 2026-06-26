namespace BusStop.UseCases.Routes.GetNearby;

public sealed record NearbyRouteDto(
    long Id,
    string Name,
    long CreatedById,
    DateTime CreatedAt,
    bool IsDeleted,
    double DistanceMeters);
