using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UseCases.Routes.Moderate;

public sealed record ModerateRouteCommand(long RouteId, ModerationCategory Category, string Reason) : ICommand<Result>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
