using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.GetMe;

public sealed class GetMeHandler(
  IReadRepository<User> repository,
  ICurrentUser currentUser) : IQueryHandler<GetMeQuery, Result<UserResponse>>
{
    public async ValueTask<Result<UserResponse>> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (currentUser.Id <= 0)
            return Result<UserResponse>.NotFound("User not found.");

        var userResult = await repository.FindRequiredAsync(new UserByIdSpec(new UserId(currentUser.Id)), "User not found.", ct);
        if (!userResult.IsSuccess)
            return Result<UserResponse>.NotFound("User not found.");
        var user = userResult.Value;

        return user.ToResponse();
    }
}
