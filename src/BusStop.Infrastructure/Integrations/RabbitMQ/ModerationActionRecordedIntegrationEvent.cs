using BusStop.Core.ModerationActionAggregate;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

// Planned for future external consumers via RabbitMQ. No in-process consumer exists
// for this event — local processing is handled via INotificationHandler<ModerationActionRecordedEvent>.
public sealed record ModerationActionRecordedIntegrationEvent
{
    public int Version { get; init; } = 1;
    public long ModerationActionId { get; init; }
    public TargetType TargetType { get; init; }
    public long TargetId { get; init; }
    public long UserId { get; init; }
    public long IssuedByUserId { get; init; }
    public ModerationCategory Category { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime IssuedAt { get; init; }
}
