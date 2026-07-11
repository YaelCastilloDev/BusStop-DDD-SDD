using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users;

public static class UserMapper
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Username?.Value, user.Email, user.ExternalId, user.CreatedAt, user.CountryId);
}
