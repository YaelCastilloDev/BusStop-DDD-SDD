using Ardalis.Specification;

namespace BusStop.Core.NotificationAggregate.Specifications;

public class UserNotificationByIdSpec : Specification<UserNotification>
{
  public UserNotificationByIdSpec(long id)
  {
    Query.Where(n => n.Id == id);
  }
}
