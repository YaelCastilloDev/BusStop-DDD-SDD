using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using Mediator;

namespace BusStop.UseCases.Notifications.Delete;

public class DeleteNotificationHandler(
  IRepository<UserNotification> repository,
  IReadRepository<User> userRepository)
  : ICommandHandler<DeleteNotificationCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result.Unauthorized("Authentication required.");

    var user = await userRepository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result.NotFound("User not found.");

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
