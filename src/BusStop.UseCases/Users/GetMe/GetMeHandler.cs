using Ardalis.Result;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.GetMe;

public sealed class GetMeHandler(IReadRepository<User> repository) : IQueryHandler<GetMeQuery, Result<UserResponse>>
{
    public async ValueTask<Result<UserResponse>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userResult = await repository.GetUserByExternalIdAsync(request.Sub, ct);
        if (!userResult.IsSuccess)
            return Result<UserResponse>.NotFound("User not found.");
        var user = userResult.Value;

        return user.ToResponse();
    }
}
