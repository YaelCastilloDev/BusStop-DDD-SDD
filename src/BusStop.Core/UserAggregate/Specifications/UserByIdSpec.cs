namespace BusStop.Core.UserAggregate.Specifications;

public sealed class UserByIdSpec : Specification<User>
{
  public UserByIdSpec(UserId userId) =>
    Query.Where(u => u.Id == userId.Value);
}
