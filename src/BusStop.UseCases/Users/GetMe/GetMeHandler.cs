using Ardalis.Result;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.GetMe;

public sealed class GetMeHandler(IReadRepository<User> repository) : IQueryHandler<GetMeQuery, Result<UserResponse>>
{
    public async ValueTask<Result<UserResponse>> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Sub))
            return Result<UserResponse>.Unauthorized("Authentication required.");

        var spec = new UserByExternalIdSpec(request.Sub);
        var user = await repository.FirstOrDefaultAsync(spec, ct);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        return new UserResponse(user.Id, user.Username?.Value, user.Email, user.ExternalId, user.CreatedAt, user.CountryId);
    }
}
