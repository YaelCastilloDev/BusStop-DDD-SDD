using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Comments.Moderate;

public sealed class ModerateCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository) : ICommandHandler<ModerateCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ModerateCommentCommand request, CancellationToken cancellationToken)
  {
    if (request.CommentId <= 0)
      return Result.Error("Comment ID is required.");
    if (string.IsNullOrEmpty(request.Sub))
      return Result.Unauthorized("Authentication required.");

    var user = await userRepository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result.NotFound("User not found. Please register first.");

    var spec = new CommentByIdSpec(new CommentId(request.CommentId));
    var comment = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (comment is null)
      return Result.NotFound("Comment not found.");

    var moderateResult = comment.Moderate(new UserId(user.Id));
    if (!moderateResult.IsSuccess)
      return Result.Error(new ErrorList(moderateResult.Errors));

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}
