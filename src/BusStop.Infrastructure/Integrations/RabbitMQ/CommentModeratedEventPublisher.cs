using BusStop.Core.CommentAggregate.Events;
using MassTransit;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

public class CommentModeratedEventPublisher(
  IPublishEndpoint publishEndpoint,
  ILogger<CommentModeratedEventPublisher> logger)
  : INotificationHandler<CommentModeratedEvent>
{
  public async ValueTask Handle(CommentModeratedEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Publishing integration event for moderated comment {CommentId}", notification.CommentId);

    var integrationEvent = new CommentModeratedIntegrationEvent
    {
      CommentId = notification.CommentId,
      UserId = notification.AuthorUserId,
      ModerationReason = "Your comment violated community guidelines.", // We can refine this later if reason is added to domain
      ModeratedAt = DateTime.UtcNow
    };

    await publishEndpoint.Publish(integrationEvent, cancellationToken);
  }
}
