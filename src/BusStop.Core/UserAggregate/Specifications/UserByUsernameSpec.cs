namespace BusStop.Core.UserAggregate.Specifications;

public sealed class UserByUsernameSpec : Specification<User>
{
    public UserByUsernameSpec(string username)
    {
        Query.Where(u => u.Username! == new Username(username));
    }
}
