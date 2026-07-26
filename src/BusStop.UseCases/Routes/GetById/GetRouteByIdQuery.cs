using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Routes.GetById;

public sealed record GetRouteByIdQuery(long RouteId) : IQuery<Result<RouteResponse>>, IIdempotentRequest;
