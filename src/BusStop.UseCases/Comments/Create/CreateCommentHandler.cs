using BusStop.Core.CommentAggregate;

namespace BusStop.UseCases.Comments.Create;

public sealed class CreateCommentHandler(IRepository<Comment> repository) : ICommandHandler<CreateCommentCommand, Result<CommentResponse>>
{
  public async ValueTask<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    var result = Comment.Create(request.Content, request.UserId, request.RouteId);
    if (!result.IsSuccess)
      return Result<CommentResponse>.Error(result.Errors.FirstOrDefault());

    var comment = result.Value;
    var created = await repository.AddAsync(comment, cancellationToken);

    return ToResponse(created);
  }

  private static CommentResponse ToResponse(Comment c) =>
    new(c.Id, c.Content.Value, c.UserId.Value, c.RouteId.Value, c.CreatedAt, c.IsDeleted,
        c.Reactions.Count(r => r.ReactionType == ReactionType.Like),
        c.Reactions.Count(r => r.ReactionType == ReactionType.Dislike));
}

