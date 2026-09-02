namespace BusStop.Core.UserAggregate.Events;

public class UserRegisteredAuditHandler(ILogger<UserRegisteredAuditHandler> logger) : INotificationHandler<UserRegisteredEvent>
{
  public ValueTask Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("User registered with email {Email} (ExternalId: {Sub})", notification.Email, notification.ExternalId);
    return ValueTask.CompletedTask;
  }
}
