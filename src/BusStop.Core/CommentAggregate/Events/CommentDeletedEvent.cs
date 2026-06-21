namespace BusStop.Core.CommentAggregate.Events;

public sealed class CommentDeletedEvent(long commentId) : DomainEventBase
{
  public long CommentId { get; } = commentId;
}
