using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;
using Mediator;

namespace BusStop.UseCases.Notifications.Delete;

public class DeleteNotificationHandler(
  IRepository<UserNotification> repository,
  IReadRepository<User> userRepository)
  : ICommandHandler<DeleteNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result.NotFound("User not found.");
    var user = userResult.Value;

    var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);

    if (notification is null)
    {
      return Result.NotFound();
    }

    if (notification.UserId.Value != user.Id)
    {
      return Result.Forbidden();
    }

    await repository.DeleteAsync(notification, cancellationToken);

    return Result.Success();
  }
}
