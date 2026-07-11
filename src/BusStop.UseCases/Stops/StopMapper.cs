using BusStop.Core.StopAggregate;

namespace BusStop.UseCases.Stops;

public static class StopMapper
{
    public static StopResponse ToResponse(this Stop stop) =>
        new(stop.Id, stop.Name.Value, stop.Location.Latitude, stop.Location.Longitude, stop.RouteId.Value, stop.IsDeleted);
}
