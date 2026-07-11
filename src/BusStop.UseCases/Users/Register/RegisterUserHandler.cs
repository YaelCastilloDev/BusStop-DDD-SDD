using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Users.Register;

public sealed class RegisterUserHandler(IRepository<User> repository) : ICommandHandler<RegisterUserCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    var existingResult = await repository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (existingResult.IsSuccess)
      return Result<UserResponse>.Error("User already registered.");

    return await repository.CreateAsync(User.Create(request.Email, request.Sub), u => u.ToResponse(), cancellationToken);
  }
}
