using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Specifications;

namespace BusStop.UseCases.Notifications.GetMy;

public class GetMyNotificationsHandler(
  IReadRepository<UserNotification> repository,
  ICurrentUser currentUser)
  : IQueryHandler<GetMyNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
  public async ValueTask<Result<IEnumerable<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var spec = new NotificationsByUserIdSpec(currentUser.Id);
    var notifications = await repository.ListAsync(spec, cancellationToken);

    var dtos = notifications.Select(n => n.ToResponse());

    return Result.Success(dtos);
  }
}
