namespace BusStop.Infrastructure.Integrations.RabbitMQ;

// TODO: Unversioned integration event. Needs versioning strategy per Gate 5 (Contract Safety).
public sealed record CommentModeratedIntegrationEvent
{
  public long CommentId { get; init; }
  public long UserId { get; init; }
  public string ModerationReason { get; init; } = string.Empty;
  public DateTime ModeratedAt { get; init; }
}
