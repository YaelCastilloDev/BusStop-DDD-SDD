using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Comments.React;

public sealed record ReactToCommentCommand(long CommentId, bool IsLike) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
