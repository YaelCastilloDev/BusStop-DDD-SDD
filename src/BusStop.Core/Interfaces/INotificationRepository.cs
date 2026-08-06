using BusStop.Core.Notifications;

namespace BusStop.Core.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(long notificationId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default);
}
