using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Register;

public sealed record OnboardingCommand(string Username, long CountryId) : ICommand<Result<UserResponse>>, IRequireAuthenticatedUser
{
  public string? Sub { get; set; }
}
