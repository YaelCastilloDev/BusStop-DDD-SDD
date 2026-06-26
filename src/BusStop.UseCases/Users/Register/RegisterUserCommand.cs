using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Register;

public sealed record RegisterUserCommand(string Email) : ICommand<Result<UserResponse>>, IRequireAuthenticatedUser
{
  public string? Sub { get; set; }
}
