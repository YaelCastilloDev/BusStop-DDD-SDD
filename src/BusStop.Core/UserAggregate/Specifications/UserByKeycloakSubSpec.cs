namespace BusStop.Core.UserAggregate.Specifications;

public sealed class UserByKeycloakSubSpec : Specification<User>
{
    public UserByKeycloakSubSpec(string keycloakSub)
    {
        Query.Where(u => u.KeycloakSub == keycloakSub);
    }
}
