using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;

namespace BusStop.Infrastructure.Data;

internal sealed class NotificationPersistence : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationPersistence(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<NotificationRecord>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(r => r.ToEntity()).ToList().AsReadOnly();
    }

    public async Task<Notification?> GetByIdAsync(long notificationId, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<NotificationRecord>()
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        return record?.ToEntity();
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var record = NotificationRecord.From(notification);
        _dbContext.Set<NotificationRecord>().Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, record.Id);
    }

    public async Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<NotificationRecord>()
            .FirstOrDefaultAsync(n => n.Id == notification.Id, cancellationToken);

        if (record is not null)
        {
            _dbContext.Set<NotificationRecord>().Remove(record);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}