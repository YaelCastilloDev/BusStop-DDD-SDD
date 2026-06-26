using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using Mediator;

namespace BusStop.UseCases.Notifications.GetMy;

public class GetMyNotificationsHandler(
  IReadRepository<UserNotification> repository,
  IReadRepository<User> userRepository)
  : IQueryHandler<GetMyNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
  public async ValueTask<Result<IEnumerable<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result.Unauthorized("Authentication required.");

    var user = await userRepository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result.NotFound("User not found.");

    var spec = new NotificationsByUserIdSpec(user.Id);
    var notifications = await repository.ListAsync(spec, cancellationToken);

    var dtos = notifications.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt));

    return Result.Success(dtos);
  }
}
