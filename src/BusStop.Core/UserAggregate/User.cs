using BusStop.Core.UserAggregate.Events;

namespace BusStop.Core.UserAggregate;

public class User : EntityBase<long>, IAggregateRoot
{
  public Username? Username { get; private set; }
  public string Email { get; private set; }
  public long? CountryId { get; private set; }
  public string? ExternalId { get; private set; }
  public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
  private User() { }
#pragma warning restore CS8618

  private User(string email, string? externalId)
  {
    Email = email;
    ExternalId = externalId;
    CreatedAt = DateTime.UtcNow;
  }

  public static User Create(string email, string externalId)
  {
    Guard.Against.NullOrWhiteSpace(email);
    Guard.Against.NullOrWhiteSpace(externalId);
    var user = new User(email, externalId);
    user.RegisterDomainEvent(new UserRegisteredEvent(user.Email, user.ExternalId!));
    return user;
  }

  public void CompleteOnboarding(Username username, long countryId)
  {
    Guard.Against.Null(username);
    Guard.Against.NegativeOrZero(countryId);
    Username = username;
    CountryId = countryId;
  }

  public void UpdateUsername(Username newUsername)
  {
    Guard.Against.Null(newUsername);
    Username = newUsername;
  }

  public void UpdateEmail(string newEmail)
  {
    Guard.Against.NullOrEmpty(newEmail);
    Email = newEmail;
  }
}
