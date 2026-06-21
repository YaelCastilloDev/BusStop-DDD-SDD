using BusStop.Core.RouteAggregate;

namespace BusStop.Core.CommentAggregate.Specifications;

public sealed class CommentsByRouteSpec : Specification<Comment>
{
  public CommentsByRouteSpec(RouteId routeId) =>
    Query.Where(c => c.RouteId == routeId);
}
