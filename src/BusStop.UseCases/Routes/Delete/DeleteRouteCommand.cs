using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Routes.Delete;

public sealed record DeleteRouteCommand(long RouteId) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
