using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Comments.Moderate;

public sealed class ModerateCommentHandler(
  IRepository<Comment> commentRepository,
  IRepository<ModerationAction> moderationActionRepository,
  ICurrentUser currentUser) : ICommandHandler<ModerateCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ModerateCommentCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var commentResult = await commentRepository.FindRequiredAsync(new CommentByIdSpec(new CommentId(request.CommentId)), "Comment not found.", cancellationToken);
    if (!commentResult.IsSuccess)
      return Result.NotFound("Comment not found.");
    var comment = commentResult.Value;

    var moderateResult = comment.Moderate(new UserId(currentUser.Id));
    if (!moderateResult.IsSuccess)
      return Result.Error(new ErrorList(moderateResult.Errors));

    var actionResult = ModerationAction.Create(TargetType.Comment, comment.Id, comment.UserId.Value, currentUser.Id, request.Category, request.Reason);
    if (!actionResult.IsSuccess)
      return Result.Error(new ErrorList(actionResult.Errors));

    var moderationAction = actionResult.Value;
    await moderationActionRepository.AddAsync(moderationAction, cancellationToken);

    await commentRepository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}
