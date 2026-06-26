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
    var route = await routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(new RouteId(request.RouteId)), cancellationToken);
    if (route is null)
      return Result<List<CommentResponse>>.NotFound("Route not found.");

    var spec = new CommentsByRouteSpec(new RouteId(request.RouteId));
    var comments = await repository.ListAsync(spec, cancellationToken);

    var responses = comments
      .Where(c => !c.IsModerated)
      .Select(c => new CommentResponse(c.Id, c.Content.Value, c.UserId.Value, c.RouteId.Value, c.CreatedAt, c.IsModerated,
          c.Reactions.Count(r => r.ReactionType == ReactionType.Like),
          c.Reactions.Count(r => r.ReactionType == ReactionType.Dislike)))
      .ToList();

    return responses;
  }
}

