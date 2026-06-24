namespace BusStop.Core.UserAggregate;

public class User : EntityBase<long>, IAggregateRoot
{
  public Username Username { get; private set; }
  public string Email { get; private set; }
  public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
  private User() { }
#pragma warning restore CS8618

  private User(Username username, string email)
  {
    Username = username;
    Email = email;
    CreatedAt = DateTime.UtcNow;
  }

  public static User Create(string username, string email)
  {
    Guard.Against.NullOrWhiteSpace(username);
    Guard.Against.NullOrWhiteSpace(email);
    return new User(new Username(username), email);
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
