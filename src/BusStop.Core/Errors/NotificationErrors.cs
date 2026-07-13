namespace BusStop.Core.Errors;

public static class NotificationErrors
{
    public const string InvalidUserId = "User ID must be positive.";
    public const string EmptyTitle = "Notification title is required.";
    public const string EmptyMessage = "Notification message is required.";
    public const string AlreadyRead = "Notification is already read.";
}
