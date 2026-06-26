using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Comments.Create;

public sealed class CreateCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository,
  IReadRepository<Route> routeRepository) : ICommandHandler<CreateCommentCommand, Result<CommentResponse>>
{
  public async ValueTask<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    var user = await userRepository.FirstOrDefaultAsync(new UserByIdSpec(new UserId(request.UserId)), cancellationToken);
    if (user is null)
      return Result<CommentResponse>.NotFound("User not found.");

    var route = await routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(new RouteId(request.RouteId)), cancellationToken);
    if (route is null)
      return Result<CommentResponse>.NotFound("Route not found.");

    var comment = Comment.Create(request.Content, request.UserId, request.RouteId);
    var created = await repository.AddAsync(comment, cancellationToken);

    return ToResponse(created);
  }

  private static CommentResponse ToResponse(Comment c) =>
    new(c.Id, c.Content.Value, c.UserId.Value, c.RouteId.Value, c.CreatedAt, c.IsDeleted,
        c.Reactions.Count(r => r.ReactionType == ReactionType.Like),
        c.Reactions.Count(r => r.ReactionType == ReactionType.Dislike));
}

