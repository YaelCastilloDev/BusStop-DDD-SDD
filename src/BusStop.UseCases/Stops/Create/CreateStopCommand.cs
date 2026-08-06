using BusStop.Core.Interfaces;

namespace BusStop.UseCases.Stops.Create;

public sealed record CreateStopCommand(string Name, double Latitude, double Longitude, long RouteId) : ICommand<Result<StopResponse>>, IRequireAuthenticatedUser
{
    public string Sub { get; set; } = default!;
}
