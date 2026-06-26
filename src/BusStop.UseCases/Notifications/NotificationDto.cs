namespace BusStop.UseCases.Notifications;

public sealed record NotificationDto(long Id, string Title, string Message, bool IsRead, DateTime CreatedAt);
