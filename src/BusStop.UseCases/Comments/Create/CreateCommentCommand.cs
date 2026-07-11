using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Comments.Create;

public sealed record CreateCommentCommand(string Content, long RouteId) : ICommand<Result<CommentResponse>>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
