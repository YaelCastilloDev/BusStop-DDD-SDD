using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.UpdateUsername;

public sealed record UpdateUsernameCommand(string NewUsername) : ICommand<Result<UserResponse>>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
