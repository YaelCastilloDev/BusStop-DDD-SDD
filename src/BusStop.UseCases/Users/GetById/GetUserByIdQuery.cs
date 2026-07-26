using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Users.GetById;

public sealed record GetUserByIdQuery(long UserId) : IQuery<Result<UserResponse>>, IIdempotentRequest;
