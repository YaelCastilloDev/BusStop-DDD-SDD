using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Users.Register;

public sealed class RegisterUserHandler(IRepository<User> repository) : ICommandHandler<RegisterUserCommand, Result<UserResponse>>
{
  public async ValueTask<Result<UserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result<UserResponse>.Unauthorized("Authentication required.");

    var existing = await repository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (existing is not null)
      return Result<UserResponse>.Error("User already registered.");

    try
    {
      var user = User.Create(request.Email, request.Sub);
      var created = await repository.AddAsync(user, cancellationToken);

      return new UserResponse(created.Id, created.Username?.Value, created.Email, created.ExternalId, created.CreatedAt, created.CountryId);
    }
    catch (ArgumentException ex)
    {
      return Result<UserResponse>.Error(ex.Message);
    }
  }
}
