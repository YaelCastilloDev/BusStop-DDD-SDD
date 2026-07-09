using BusStop.Core.Exceptions;
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

  public static Stop Create(string name, double latitude, double longitude, long routeId)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new DomainValidationException("Stop name is required.", nameof(name));
    if (latitude < -90 || latitude > 90)
      throw new DomainValidationException("Latitude must be between -90 and 90.", nameof(latitude));
    if (longitude < -180 || longitude > 180)
      throw new DomainValidationException("Longitude must be between -180 and 180.", nameof(longitude));
    if (routeId <= 0)
      throw new DomainValidationException("RouteId must be positive.", nameof(routeId));

    return new Stop(new StopName(name), new Location(latitude, longitude), new RouteId(routeId));
  }

  public void UpdateName(StopName newName)
  {
    if (newName is null)
      throw new DomainValidationException("New stop name is required.", nameof(newName));
    Name = newName;
  }

  public void UpdateLocation(Location newLocation)
  {
    if (newLocation is null)
      throw new DomainValidationException("New location is required.", nameof(newLocation));
    Location = newLocation;
  }

  public void Delete(UserId deletedBy)
  {
    if (deletedBy is null)
      throw new DomainValidationException("DeletedBy is required.", nameof(deletedBy));
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy.Value;
    RegisterDomainEvent(new StopDeletedEvent(Id));
  }

  public bool IsDeleted => DeletedAt.HasValue;
}
