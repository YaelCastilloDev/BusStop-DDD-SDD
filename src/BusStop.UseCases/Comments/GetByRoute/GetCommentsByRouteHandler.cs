using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Comments.GetByRoute;

public sealed class GetCommentsByRouteHandler(IReadRepository<Comment> repository) : IQueryHandler<GetCommentsByRouteQuery, Result<List<CommentResponse>>>
{
  public async ValueTask<Result<List<CommentResponse>>> Handle(GetCommentsByRouteQuery request, CancellationToken cancellationToken)
  {
    var spec = new CommentsByRouteSpec(new RouteId(request.RouteId));
    var comments = await repository.ListAsync(spec, cancellationToken);

    var responses = comments
      .Where(c => !c.IsDeleted)
      .Select(c => new CommentResponse(c.Id, c.Content.Value, c.UserId.Value, c.RouteId.Value, c.CreatedAt, c.IsDeleted,
          c.Reactions.Count(r => r.ReactionType == ReactionType.Like),
          c.Reactions.Count(r => r.ReactionType == ReactionType.Dislike)))
      .ToList();

    return responses;
  }
}

