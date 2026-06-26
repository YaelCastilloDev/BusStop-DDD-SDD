using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.Create;

public sealed class CreateUserHandler(IRepository<User> repository) : ICommandHandler<CreateUserCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result<UserResponse>.Unauthorized("Authentication required.");

    try
    {
      var user = User.Create(request.Username, request.Email, request.Sub);
      var created = await repository.AddAsync(user, cancellationToken);

      return new UserResponse(created.Id, created.Username.Value, created.Email, created.KeycloakSub, created.CreatedAt);
    }
    catch (ArgumentException ex)
    {
      return Result<UserResponse>.Error(ex.Message);
    }
  }
}

