namespace BusStop.UseCases.Stops.Create;

public sealed record CreateStopCommand(string Name, double Latitude, double Longitude, long RouteId) : ICommand<Result<StopResponse>>;
