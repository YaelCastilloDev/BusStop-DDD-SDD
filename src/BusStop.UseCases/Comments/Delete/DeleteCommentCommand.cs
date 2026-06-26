using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Comments.Delete;

public sealed record DeleteCommentCommand(long CommentId) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string? Sub { get; set; }
}
