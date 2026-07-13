using BusStop.Infrastructure.Integrations.RabbitMQ;
using BusStop.UseCases.Notifications.ConsumeModerated;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace BusStop.Web.Notifications;

public class ModerationActionRecordedEventConsumer(
  IMediator mediator,
  IHubContext<NotificationsHub> hubContext,
  ILogger<ModerationActionRecordedEventConsumer> logger)
  : IConsumer<ModerationActionRecordedIntegrationEvent>
{
  private readonly IMediator _mediator = mediator;
  private readonly IHubContext<NotificationsHub> _hubContext = hubContext;
  private readonly ILogger<ModerationActionRecordedEventConsumer> _logger = logger;

  public async Task Consume(ConsumeContext<ModerationActionRecordedIntegrationEvent> context)
  {
    var msg = context.Message;
    _logger.LogInformation("Consumed ModerationActionRecordedIntegrationEvent for {TargetType} {TargetId}",
        msg.TargetType, msg.TargetId);

    var command = new ProcessModerationNotificationCommand(
        msg.UserId,
        msg.TargetType,
        msg.TargetId,
        msg.Reason,
        msg.Category);
    var result = await _mediator.Send(command, context.CancellationToken);

    if (result.IsSuccess)
    {
      // Push via SignalR
      await _hubContext.Clients.User(msg.UserId.ToString()).SendAsync(NotificationsHub.ReceiveNotificationMethod, new
      {
        Title = "Your content was moderated",
        Message = $"Your {msg.TargetType.ToString().ToLower()} (ID: {msg.TargetId}) was moderated. Reason: {msg.Reason}",
        CreatedAt = DateTime.UtcNow
      }, context.CancellationToken);
    }
    else
    {
      _logger.LogWarning("Failed to process moderation notification for User {UserId}", msg.UserId);
    }
  }
}
