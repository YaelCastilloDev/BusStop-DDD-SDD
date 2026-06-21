namespace BusStop.Core.CommentAggregate.Events;

public class CommentDeletedHandler(ILogger<CommentDeletedHandler> logger) : INotificationHandler<CommentDeletedEvent>
{
  public ValueTask Handle(CommentDeletedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Comment {CommentId} was deleted", notification.CommentId);
    return ValueTask.CompletedTask;
  }
}
