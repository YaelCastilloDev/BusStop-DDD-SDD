using BusStop.Core.Notifications;

namespace BusStop.Infrastructure.Data;

internal sealed class NotificationRecord
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    internal static NotificationRecord From(Notification notification) => new()
    {
        Id = notification.Id,
        UserId = notification.UserId,
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };

    internal Notification ToEntity()
    {
        var notification = new Notification(UserId, Title, Message);
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, Id);
        if (IsRead) notification.MarkAsRead();
        return notification;
    }
}
