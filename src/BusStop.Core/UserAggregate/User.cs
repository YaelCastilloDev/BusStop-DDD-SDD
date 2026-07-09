using BusStop.Core.Exceptions;
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
    if (string.IsNullOrWhiteSpace(email))
      throw new DomainValidationException("Email is required.", nameof(email));
    if (string.IsNullOrWhiteSpace(externalId))
      throw new DomainValidationException("ExternalId is required.", nameof(externalId));
    var user = new User(email, externalId);
    user.RegisterDomainEvent(new UserRegisteredEvent(user.Email, user.ExternalId!));
    return user;
  }

  public void CompleteOnboarding(Username username, long countryId)
  {
    if (username is null)
      throw new DomainValidationException("Username is required.", nameof(username));
    if (countryId <= 0)
      throw new DomainValidationException("CountryId must be positive.", nameof(countryId));
    Username = username;
    CountryId = countryId;
  }

  public void UpdateUsername(Username newUsername)
  {
    if (newUsername is null)
      throw new DomainValidationException("New username is required.", nameof(newUsername));
    Username = newUsername;
  }

  public void UpdateEmail(string newEmail)
  {
    if (string.IsNullOrEmpty(newEmail))
      throw new DomainValidationException("New email is required.", nameof(newEmail));
    Email = newEmail;
  }
}
