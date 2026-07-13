using BusStop.Core.ModerationActionAggregate;

namespace BusStop.Infrastructure.Integrations.RabbitMQ;

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
