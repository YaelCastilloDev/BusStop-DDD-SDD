namespace BusStop.UseCases.Stops;

public sealed record StopResponse(long Id, string Name, double Latitude, double Longitude, long RouteId, bool IsDeleted);
