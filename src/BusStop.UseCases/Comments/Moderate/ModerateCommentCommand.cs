using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Comments.Moderate;

public sealed record ModerateCommentCommand(long CommentId) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string? Sub { get; set; }
}
