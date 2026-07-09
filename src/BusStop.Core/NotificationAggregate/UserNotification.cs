using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.NotificationAggregate;

public class UserNotification : EntityBase<long>, IAggregateRoot
{
  public UserId UserId { get; private set; }
  public string Title { get; private set; }
  public string Message { get; private set; }
  public bool IsRead { get; private set; }
  public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
  private UserNotification() { }
#pragma warning restore CS8618

  private UserNotification(UserId userId, string title, string message)
  {
    UserId = userId;
    Title = title;
    Message = message;
    IsRead = false;
    CreatedAt = DateTime.UtcNow;
  }

  public static UserNotification Create(long userId, string title, string message)
  {
    if (userId <= 0)
      throw new DomainValidationException("UserId must be positive.", nameof(userId));
    if (string.IsNullOrWhiteSpace(title))
      throw new DomainValidationException("Notification title is required.", nameof(title));
    if (string.IsNullOrWhiteSpace(message))
      throw new DomainValidationException("Notification message is required.", nameof(message));

    return new UserNotification(new UserId(userId), title, message);
  }

  public void MarkAsRead()
  {
    if (!IsRead)
    {
      IsRead = true;
    }
  }
}
