using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;

namespace BusStop.UseCases.Comments.React;

public sealed class ReactToCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository) : ICommandHandler<ReactToCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result.NotFound("User not found.");
    var user = userResult.Value;

    var commentResult = await repository.FindRequiredAsync(new CommentByIdSpec(new CommentId(request.CommentId)), "Comment not found.", cancellationToken);
    if (!commentResult.IsSuccess)
      return Result.NotFound("Comment not found.");
    var comment = commentResult.Value;

    if (comment.IsModerated)
      return Result.Error("Cannot react to a moderated comment.");

    if (!Enum.TryParse<ReactionType>(request.ReactionType, ignoreCase: true, out var reactionType))
      reactionType = ReactionType.Like;

    comment.AddReaction(new UserId(user.Id), reactionType);

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}

