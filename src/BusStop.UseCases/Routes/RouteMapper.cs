using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Routes;

public static class RouteMapper
{
    public static RouteResponse ToResponse(this Route route) =>
        new(route.Id, route.Name.Value, route.CreatedById.Value, route.CreatedAt, route.IsDeleted);
}
