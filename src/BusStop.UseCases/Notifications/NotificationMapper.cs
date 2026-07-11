using BusStop.Core.NotificationAggregate;

namespace BusStop.UseCases.Notifications;

public static class NotificationMapper
{
    public static NotificationDto ToResponse(this UserNotification notification) =>
        new(notification.Id, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt);
}
