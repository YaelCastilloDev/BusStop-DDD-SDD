using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Routes.Create;

public sealed record CreateRouteCommand(string Name) : ICommand<Result<RouteResponse>>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
