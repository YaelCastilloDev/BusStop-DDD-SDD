namespace BusStop.Core.RouteAggregate.Specifications;

public sealed class RouteByIdSpec : Specification<Route>
{
  public RouteByIdSpec(RouteId routeId) =>
    Query.Where(r => r.Id == routeId.Value);
}
