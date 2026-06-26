namespace BusStop.Core.UserAggregate.Specifications;

public sealed class UserByExternalIdSpec : Specification<User>
{
    public UserByExternalIdSpec(string externalId)
    {
        Query.Where(u => u.ExternalId == externalId);
    }
}
