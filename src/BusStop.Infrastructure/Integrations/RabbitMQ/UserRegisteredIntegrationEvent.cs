namespace BusStop.Infrastructure.Integrations.RabbitMQ;

// TODO: Unversioned integration event. Needs versioning strategy per Gate 5 (Contract Safety).
public sealed record UserRegisteredIntegrationEvent
{
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
