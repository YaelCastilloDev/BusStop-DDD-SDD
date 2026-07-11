using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;

namespace BusStop.UseCases.Comments.GetByRoute;

public sealed class GetCommentsByRouteHandler(
  IReadRepository<Comment> repository,
  IReadRepository<Route> routeRepository) : IQueryHandler<GetCommentsByRouteQuery, Result<List<CommentResponse>>>
{
  public async ValueTask<Result<List<CommentResponse>>> Handle(GetCommentsByRouteQuery request, CancellationToken cancellationToken)
  {
    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<List<CommentResponse>>.NotFound("Route not found.");
    var route = routeResult.Value;

    var spec = new CommentsByRouteSpec(new RouteId(request.RouteId));
    var comments = await repository.ListAsync(spec, cancellationToken);

    var responses = comments
      .Where(c => !c.IsModerated)
      .Select(c => c.ToResponse())
      .ToList();

    return responses;
  }
}

