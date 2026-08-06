namespace BusStop.Core.Notifications;

public class Notification : EntityBase<long>
{
    public long UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
    private Notification() { }
#pragma warning restore CS8618

    public Notification(long userId, string title, string message)
    {
        Guard.Against.NegativeOrZero(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(message, nameof(message));

        UserId = userId;
        Title = title;
        Message = message;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
