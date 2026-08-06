using BusStop.Core.CommentAggregate;

namespace BusStop.UseCases.Comments;

public static class CommentMapper
{
    public static CommentResponse ToResponse(this Comment comment) =>
        new(comment.Id, comment.Content.Value, comment.UserId.Value, comment.RouteId, comment.CreatedAt, comment.IsModerated,
            comment.Reactions.Count(r => r.IsLike),
            comment.Reactions.Count(r => !r.IsLike));
}
