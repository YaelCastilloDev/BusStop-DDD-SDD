namespace BusStop.UseCases.Routes.Delete;

public sealed record DeleteRouteCommand(long RouteId, long DeletedById) : ICommand<Result>;
