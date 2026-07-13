using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UseCases.Comments.Moderate;

public sealed record ModerateCommentCommand(long CommentId, ModerationCategory Category, string Reason) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
