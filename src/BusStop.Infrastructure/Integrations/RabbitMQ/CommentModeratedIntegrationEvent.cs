namespace BusStop.Infrastructure.Integrations.RabbitMQ;

public sealed record CommentModeratedIntegrationEvent
{
  public long CommentId { get; init; }
  public long UserId { get; init; }
  public string ModerationReason { get; init; } = string.Empty;
  public DateTime ModeratedAt { get; init; }
}
