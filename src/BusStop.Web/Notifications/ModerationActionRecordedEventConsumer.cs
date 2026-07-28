using BusStop.Core.ModerationActionAggregate.Events;
using BusStop.UseCases.Notifications.ConsumeModerated;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BusStop.Web.Notifications;

public class ModerationActionRecordedEventConsumer(
  IMediator mediator,
  IHubContext<NotificationsHub> hubContext,
  ILogger<ModerationActionRecordedEventConsumer> logger)
  : INotificationHandler<ModerationActionRecordedEvent>
{
  private readonly IMediator _mediator = mediator;
  private readonly IHubContext<NotificationsHub> _hubContext = hubContext;
  private readonly ILogger<ModerationActionRecordedEventConsumer> _logger = logger;

  public async ValueTask Handle(ModerationActionRecordedEvent notification, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling ModerationActionRecordedEvent for {TargetType} {TargetId}",
        notification.TargetType, notification.TargetId);

    var command = new ProcessModerationNotificationCommand(
        notification.UserId,
        notification.TargetType,
        notification.TargetId,
        notification.Reason,
        notification.Category);
    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync(NotificationsHub.ReceiveNotificationMethod, new
      {
        Title = "Your content was moderated",
        Message = $"Your {notification.TargetType.ToString().ToLower()} (ID: {notification.TargetId}) was moderated. Reason: {notification.Reason}",
        CreatedAt = DateTime.UtcNow
      }, cancellationToken);
    }
    else
    {
      _logger.LogWarning("Failed to process moderation notification for User {UserId}", notification.UserId);
    }
  }
}
