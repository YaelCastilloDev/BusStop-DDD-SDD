using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Comments.React;

public sealed record ReactToCommentCommand(long CommentId, string ReactionType) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string? Sub { get; set; }
}
