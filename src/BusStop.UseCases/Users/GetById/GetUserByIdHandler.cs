using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.GetById;

public sealed class GetUserByIdHandler(IReadRepository<User> repository) : IQueryHandler<GetUserByIdQuery, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var spec = new UserByIdSpec(new UserId(request.UserId));
    var user = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (user is null)
      return Result<UserResponse>.NotFound("User not found.");

    return new UserResponse(user.Id, user.Username?.Value, user.Email, user.ExternalId, user.CreatedAt, user.CountryId);
  }
}
