namespace BusStop.UseCases.Stops.GetByRoute;

public sealed record GetStopsByRouteQuery(long RouteId) : IQuery<Result<List<StopResponse>>>;
