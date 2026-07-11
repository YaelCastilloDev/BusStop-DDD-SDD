using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;

namespace BusStop.UseCases.Comments.Moderate;

public sealed class ModerateCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository) : ICommandHandler<ModerateCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ModerateCommentCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result.NotFound("User not found.");
    var user = userResult.Value;

    var commentResult = await repository.FindRequiredAsync(new CommentByIdSpec(new CommentId(request.CommentId)), "Comment not found.", cancellationToken);
    if (!commentResult.IsSuccess)
      return Result.NotFound("Comment not found.");
    var comment = commentResult.Value;

    var moderateResult = comment.Moderate(new UserId(user.Id));
    if (!moderateResult.IsSuccess)
      return Result.Error(new ErrorList(moderateResult.Errors));

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}
