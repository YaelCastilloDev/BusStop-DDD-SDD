using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Comments.React;

public sealed class ReactToCommentHandler(IRepository<Comment> repository) : ICommandHandler<ReactToCommentCommand, Result>
{
  public async ValueTask<Result> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
  {
    var spec = new CommentByIdSpec(new CommentId(request.CommentId));
    var comment = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (comment is null)
      return Result.NotFound("Comment not found.");

    if (comment.IsDeleted)
      return Result.Error("Cannot react to a deleted comment.");

    if (!Enum.TryParse<ReactionType>(request.ReactionType, ignoreCase: true, out var reactionType))
      reactionType = ReactionType.Like;

    comment.AddReaction(new UserId(request.UserId), reactionType);

    await repository.UpdateAsync(comment, cancellationToken);

    return Result.Success();
  }
}

