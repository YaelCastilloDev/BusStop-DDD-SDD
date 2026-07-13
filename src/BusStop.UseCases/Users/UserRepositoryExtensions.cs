using Ardalis.Result;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users;

public static class UserRepositoryExtensions
{
    public static async Task<Result<User>> GetUserByExternalIdAsync(
        this IRepository<User> repository, string sub, CancellationToken ct)
    {
        var spec = new UserByExternalIdSpec(sub);
        var user = await repository.FirstOrDefaultAsync(spec, ct);
        return user is null
            ? Result<User>.NotFound("User not found.")
            : Result<User>.Success(user);
    }
}
