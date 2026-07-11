using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;
using Mediator;

namespace BusStop.UseCases.Notifications.GetMy;

public class GetMyNotificationsHandler(
  IReadRepository<UserNotification> repository,
  IReadRepository<User> userRepository)
  : IQueryHandler<GetMyNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
  public async ValueTask<Result<IEnumerable<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result.NotFound("User not found.");
    var user = userResult.Value;

    var spec = new NotificationsByUserIdSpec(user.Id);
    var notifications = await repository.ListAsync(spec, cancellationToken);

    var dtos = notifications.Select(n => n.ToResponse());

    return Result.Success(dtos);
  }
}
