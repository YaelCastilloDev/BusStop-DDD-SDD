using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Comments.Delete;

public sealed class DeleteCommentHandler(IRepository<Comment> repository) : ICommandHandler<DeleteCommentCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
  {
    var spec = new CommentByIdSpec(new CommentId(request.CommentId));
    var comment = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (comment is null)
      return Result.NotFound("Comment not found.");

    if (comment.IsDeleted)
      return Result.Error("Comment is already deleted.");

    comment.Delete(new UserId(request.DeletedById));

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}

