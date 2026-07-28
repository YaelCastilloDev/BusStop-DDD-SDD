using BusStop.Core.ModerationActionAggregate.Events;
using MassTransit;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

// Publishes ModerationActionRecordedIntegrationEvent to RabbitMQ for future external
// consumers. Local processing (notification creation + email + SignalR push) is handled
// in-process by ModerationActionRecordedEventConsumer in the Web layer.
public class ModerationActionRecordedEventPublisher(
    IPublishEndpoint publishEndpoint,
    ILogger<ModerationActionRecordedEventPublisher> logger)
    : INotificationHandler<ModerationActionRecordedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<ModerationActionRecordedEventPublisher> _logger = logger;

    public async ValueTask Handle(ModerationActionRecordedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing integration event for moderation action {ModerationActionId}",
            notification.ModerationActionId);

        var integrationEvent = new ModerationActionRecordedIntegrationEvent
        {
            Version = 1,
            ModerationActionId = notification.ModerationActionId,
            TargetType = notification.TargetType,
            TargetId = notification.TargetId,
            UserId = notification.UserId,
            IssuedByUserId = notification.IssuedByUserId,
            Category = notification.Category,
            Reason = notification.Reason,
            IssuedAt = notification.IssuedAt
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
