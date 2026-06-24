using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.Create;

public sealed class CreateUserHandler(IRepository<User> repository) : ICommandHandler<CreateUserCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var user = User.Create(request.Username, request.Email);
      var created = await repository.AddAsync(user, cancellationToken);

      return new UserResponse(created.Id, created.Username.Value, created.Email, created.CreatedAt);
    }
    catch (ArgumentException ex)
    {
      return Result<UserResponse>.Error(ex.Message);
    }
  }
}

