using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;

namespace BusStop.UseCases.Notifications.GetMy;

public class GetMyNotificationsHandler(
  INotificationRepository notificationRepository,
  ICurrentUser currentUser)
  : IQueryHandler<GetMyNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
  public async ValueTask<Result<IEnumerable<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var notifications = await notificationRepository.GetByUserIdAsync(currentUser.Id, cancellationToken);
    var dtos = notifications.Select(n => n.ToResponse());
    return Result.Success(dtos);
  }
}
