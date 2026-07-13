namespace BusStop.Core.ModerationActionAggregate.Events;

public sealed class ModerationActionRecordedEvent(
    long moderationActionId,
    TargetType targetType,
    long targetId,
    long userId,
    long issuedByUserId,
    ModerationCategory category,
    string reason,
    DateTime issuedAt) : DomainEventBase
{
    public long ModerationActionId { get; } = moderationActionId;
    public TargetType TargetType { get; } = targetType;
    public long TargetId { get; } = targetId;
    public long UserId { get; } = userId;
    public long IssuedByUserId { get; } = issuedByUserId;
    public ModerationCategory Category { get; } = category;
    public string Reason { get; } = reason;
    public DateTime IssuedAt { get; } = issuedAt;
}
