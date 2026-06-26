using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Comments.Delete;

public sealed class DeleteCommentHandler(
  IRepository<Comment> repository,
  IReadRepository<User> userRepository) : ICommandHandler<DeleteCommentCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
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

    if (comment.IsDeleted)
      return Result.Error("Comment is already deleted.");

    comment.Delete(new UserId(user.Id));

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}

