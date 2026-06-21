namespace BusStop.UseCases.Comments.Create;

public sealed record CreateCommentCommand(string Content, long UserId, long RouteId) : ICommand<Result<CommentResponse>>;
