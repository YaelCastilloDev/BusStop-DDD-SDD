namespace BusStop.Core.CommentAggregate.Events;

public sealed class CommentModeratedEvent(long commentId, long authorUserId) : DomainEventBase
{
  public long CommentId { get; } = commentId;
  public long AuthorUserId { get; } = authorUserId;
}
