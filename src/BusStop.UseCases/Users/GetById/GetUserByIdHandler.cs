using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.GetById;

public sealed class GetUserByIdHandler(IReadRepository<User> repository) : IQueryHandler<GetUserByIdQuery, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var userResult = await repository.FindRequiredAsync(new UserByIdSpec(new UserId(request.UserId)), "User not found.", cancellationToken);
    if (!userResult.IsSuccess)
      return Result<UserResponse>.NotFound("User not found.");
    var user = userResult.Value;

    return user.ToResponse();
  }
}
