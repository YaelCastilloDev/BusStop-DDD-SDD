namespace BusStop.UseCases.Routes.Create;

public sealed record CreateRouteCommand(string Name, long CreatedById) : ICommand<Result<RouteResponse>>;
