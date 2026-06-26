namespace BusStop.Infrastructure.Integrations.RabbitMQ;

public sealed record UserRegisteredIntegrationEvent
{
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
