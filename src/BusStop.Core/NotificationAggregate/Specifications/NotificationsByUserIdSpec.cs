using Ardalis.Specification;

namespace BusStop.Core.NotificationAggregate.Specifications;

public class NotificationsByUserIdSpec : Specification<UserNotification>
{
  public NotificationsByUserIdSpec(long userId)
  {
    Query.Where(n => n.UserId.Value == userId)
         .OrderByDescending(n => n.CreatedAt);
  }
}
