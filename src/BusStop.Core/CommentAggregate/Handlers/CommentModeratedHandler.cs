namespace BusStop.Core.CommentAggregate.Handlers;

public class CommentModeratedHandler(ILogger<CommentModeratedHandler> logger) : INotificationHandler<CommentModeratedEvent>
{
  public ValueTask Handle(CommentModeratedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Comment {CommentId} was moderated", notification.CommentId);
    return ValueTask.CompletedTask;
  }
}
