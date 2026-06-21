namespace BusStop.UseCases.Comments.Delete;

public sealed record DeleteCommentCommand(long CommentId, long DeletedById) : ICommand<Result>;
