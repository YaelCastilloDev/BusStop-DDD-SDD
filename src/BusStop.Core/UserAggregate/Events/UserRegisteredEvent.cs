namespace BusStop.Core.UserAggregate.Events;

public sealed class UserRegisteredEvent(string email, string externalId) : DomainEventBase
{
  public string Email { get; } = email;
  public string ExternalId { get; } = externalId;
}
