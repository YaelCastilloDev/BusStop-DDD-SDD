using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.GetMe;

public sealed record GetMeQuery() : IQuery<Result<UserResponse>>, IRequireAuthenticatedUser, IIdempotentRequest
{
    public string Sub { get; set; } = default!;
}
