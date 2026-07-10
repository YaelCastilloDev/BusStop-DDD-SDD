using BusStop.Core.Errors;
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
        Guard.Against.Null(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(message, nameof(message));

        UserId = userId;
        Title = title;
        Message = message;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<UserNotification> Create(long userId, string title, string message)
    {
        var errors = new List<string>();

        if (userId <= 0)
            errors.Add(NotificationErrors.InvalidUserId);
        if (string.IsNullOrWhiteSpace(title))
            errors.Add(NotificationErrors.EmptyTitle);
        if (string.IsNullOrWhiteSpace(message))
            errors.Add(NotificationErrors.EmptyMessage);

        if (errors.Count > 0)
            return Result<UserNotification>.Error(new ErrorList(errors));

        return Result<UserNotification>.Success(new UserNotification(new UserId(userId), title, message));
    }

    public void MarkAsRead()
    {
        if (!IsRead)
            IsRead = true;
    }
}
