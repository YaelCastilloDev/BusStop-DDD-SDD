using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate.Events;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.StopAggregate;

public class Stop : EntityBase<long>, IAggregateRoot
{
  public StopName Name { get; private set; }
  public Location Location { get; private set; }
  public RouteId RouteId { get; private set; }
  public DateTime? DeletedAt { get; private set; }
  public long? DeletedBy { get; private set; }

#pragma warning disable CS8618
  private Stop() { }
#pragma warning restore CS8618

  private Stop(StopName name, Location location, RouteId routeId)
  {
    Name = name;
    Location = location;
    RouteId = routeId;
  }

  public static Result<Stop> Create(string name, double latitude, double longitude, long routeId)
  {
    if (string.IsNullOrWhiteSpace(name))
      return Result<Stop>.Error("Stop name is required.");
    if (latitude < -90 || latitude > 90)
      return Result<Stop>.Error("Latitude must be between -90 and 90.");
    if (longitude < -180 || longitude > 180)
      return Result<Stop>.Error("Longitude must be between -180 and 180.");
    if (routeId <= 0)
      return Result<Stop>.Error("Route ID is required.");

    return Result<Stop>.Success(new Stop(new StopName(name), new Location(latitude, longitude), new RouteId(routeId)));
  }

  public void UpdateName(StopName newName)
  {
    Guard.Against.Null(newName);
    Name = newName;
  }

  public void UpdateLocation(Location newLocation)
  {
    Guard.Against.Null(newLocation);
    Location = newLocation;
  }

  public void Delete(UserId deletedBy)
  {
    Guard.Against.Null(deletedBy);
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy.Value;
    RegisterDomainEvent(new StopDeletedEvent(Id));
  }

  public bool IsDeleted => DeletedAt.HasValue;
}
