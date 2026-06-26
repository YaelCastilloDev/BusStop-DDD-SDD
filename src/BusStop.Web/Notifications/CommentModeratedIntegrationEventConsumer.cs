using BusStop.Infrastructure.Integrations.RabbitMQ;
using BusStop.UseCases.Notifications.ConsumeModerated;
using MassTransit;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BusStop.Web.Notifications;

public class CommentModeratedIntegrationEventConsumer(
  IMediator mediator,
  IHubContext<NotificationsHub> hubContext,
  ILogger<CommentModeratedIntegrationEventConsumer> logger)
  : IConsumer<CommentModeratedIntegrationEvent>
{
  public async Task Consume(ConsumeContext<CommentModeratedIntegrationEvent> context)
  {
    var msg = context.Message;
    logger.LogInformation("Consumed CommentModeratedIntegrationEvent for Comment {CommentId}", msg.CommentId);

    var command = new ProcessModerationNotificationCommand(msg.UserId, msg.CommentId, msg.ModerationReason);
    var result = await mediator.Send(command, context.CancellationToken);

    if (result.IsSuccess)
    {
      // Push via SignalR
      await hubContext.Clients.User(msg.UserId.ToString()).SendAsync(NotificationsHub.ReceiveNotificationMethod, new
      {
        Title = "Your comment was moderated",
        Message = $"Your comment (ID: {msg.CommentId}) was moderated. Reason: {msg.ModerationReason}",
        CreatedAt = DateTime.UtcNow
      }, context.CancellationToken);
    }
    else
    {
      logger.LogWarning("Failed to process moderation notification for User {UserId}", msg.UserId);
    }
  }
}
