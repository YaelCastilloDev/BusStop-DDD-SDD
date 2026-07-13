using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Specifications;

namespace BusStop.UseCases.Notifications.Delete;

public class DeleteNotificationHandler(
  IRepository<UserNotification> repository,
  ICurrentUser currentUser)
  : ICommandHandler<DeleteNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var notificationResult = await repository.FindRequiredAsync(
      new UserNotificationByIdSpec(request.NotificationId),
      "Notification not found.",
      cancellationToken);

    if (!notificationResult.IsSuccess)
      return Result.NotFound();

    var notification = notificationResult.Value;

    if (notification.UserId.Value != currentUser.Id)
    {
      return Result.Forbidden();
    }

    await repository.DeleteAsync(notification, cancellationToken);

    return Result.Success();
  }
}
