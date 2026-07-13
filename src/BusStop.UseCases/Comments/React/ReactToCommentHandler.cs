using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Comments.React;

public sealed class ReactToCommentHandler(
  IRepository<Comment> repository,
  ICurrentUser currentUser) : ICommandHandler<ReactToCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var commentResult = await repository.FindRequiredAsync(new CommentByIdSpec(new CommentId(request.CommentId)), "Comment not found.", cancellationToken);
    if (!commentResult.IsSuccess)
      return Result.NotFound("Comment not found.");
    var comment = commentResult.Value;

    if (comment.IsModerated)
      return Result.Error("Cannot react to a moderated comment.");

    if (!Enum.TryParse<ReactionType>(request.ReactionType, ignoreCase: true, out var reactionType))
      reactionType = ReactionType.Like;

    _ = comment.AddReaction(new UserId(currentUser.Id), reactionType);

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}
