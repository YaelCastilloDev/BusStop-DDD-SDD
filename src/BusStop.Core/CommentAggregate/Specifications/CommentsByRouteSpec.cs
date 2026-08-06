namespace BusStop.Core.CommentAggregate.Specifications;

public sealed class CommentsByRouteSpec : Specification<Comment>
{
  public CommentsByRouteSpec(long routeId) =>
    Query.Where(c => c.RouteId == routeId);
}
