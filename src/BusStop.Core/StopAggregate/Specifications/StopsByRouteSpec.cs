namespace BusStop.Core.StopAggregate.Specifications;

public sealed class StopsByRouteSpec : Specification<Stop>
{
  public StopsByRouteSpec(long routeId) =>
    Query.Where(s => s.RouteId == routeId);
}
