namespace BusStop.UseCases.Comments;

public sealed record CommentResponse(long Id, string Content, long UserId, long RouteId, DateTime CreatedAt, bool IsDeleted, int LikeCount, int DislikeCount);
