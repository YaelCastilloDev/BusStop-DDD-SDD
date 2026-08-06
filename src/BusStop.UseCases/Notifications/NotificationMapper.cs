using BusStop.Core.Notifications;

namespace BusStop.UseCases.Notifications;

public static class NotificationMapper
{
    public static NotificationDto ToResponse(this Notification notification) =>
        new(notification.Id, notification.UserId, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt);
}
