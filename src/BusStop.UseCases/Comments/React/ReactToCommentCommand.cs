namespace BusStop.UseCases.Comments.React;

public sealed record ReactToCommentCommand(long CommentId, long UserId, string ReactionType) : ICommand<Result>;
