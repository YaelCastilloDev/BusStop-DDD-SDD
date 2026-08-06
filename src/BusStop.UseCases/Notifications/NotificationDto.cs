namespace BusStop.UseCases.Notifications;

public sealed record NotificationDto(long Id, long UserId, string Title, string Message, bool IsRead, DateTime CreatedAt);
