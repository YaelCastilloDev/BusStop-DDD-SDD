using BusStop.Core.RouteAggregate;

namespace BusStop.Core.StopAggregate.Specifications;

public sealed class StopsByRouteSpec : Specification<Stop>
{
  public StopsByRouteSpec(RouteId routeId) =>
    Query.Where(s => s.RouteId == routeId);
}
