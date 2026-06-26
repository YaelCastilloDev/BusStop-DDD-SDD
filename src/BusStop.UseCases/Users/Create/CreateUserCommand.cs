using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.Create;

public sealed record CreateUserCommand(string Username, string Email) : ICommand<Result<UserResponse>>, IRequireAuthenticatedUser
{
    public string? Sub { get; set; }
}
