namespace BusStop.Core.CommentAggregate.Events;

public sealed class CommentModeratedEvent(long commentId) : DomainEventBase
{
  public long CommentId { get; } = commentId;
}
