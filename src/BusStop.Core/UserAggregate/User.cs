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

  public static Result<User> Create(string username, string email)
  {
    if (string.IsNullOrWhiteSpace(username))
      return Result<User>.Error("Username is required.");
    if (string.IsNullOrWhiteSpace(email))
      return Result<User>.Error("Email is required.");

    return Result<User>.Success(new User(new Username(username), email));
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
