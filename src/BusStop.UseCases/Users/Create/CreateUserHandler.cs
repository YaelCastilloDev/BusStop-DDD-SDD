using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.Create;

public sealed class CreateUserHandler(IRepository<User> repository) : ICommandHandler<CreateUserCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    var result = User.Create(request.Username, request.Email);
    if (!result.IsSuccess)
      return Result<UserResponse>.Error(result.Errors.FirstOrDefault());

    var user = result.Value;
    var created = await repository.AddAsync(user, cancellationToken);

    return new UserResponse(created.Id, created.Username.Value, created.Email, created.CreatedAt);
  }
}

