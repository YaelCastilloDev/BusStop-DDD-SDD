namespace BusStop.Core.CommentAggregate.Events;

public sealed class CommentModeratedEvent(long commentId, long authorUserId, long moderatorUserId) : DomainEventBase
{
    public long CommentId { get; } = commentId;
    public long AuthorUserId { get; } = authorUserId;
    public long ModeratorUserId { get; } = moderatorUserId;
}
