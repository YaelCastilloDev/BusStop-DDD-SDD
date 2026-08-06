using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;

namespace BusStop.UseCases.Notifications.Delete;

public class DeleteNotificationHandler(
  INotificationRepository notificationRepository,
  ICurrentUser currentUser)
  : ICommandHandler<DeleteNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
  {
    var notification = await notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
    if (notification is null)
      return Result.NotFound("Notification not found.");

    if (notification.UserId != currentUser.Id)
      return Result.Forbidden("You can only delete your own notifications.");

    await notificationRepository.DeleteAsync(notification, cancellationToken);
    return Result.Success();
  }
}
