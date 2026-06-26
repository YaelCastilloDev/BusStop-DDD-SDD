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
    Guard.Against.NullOrWhiteSpace(name);
    Guard.Against.OutOfRange(latitude, nameof(latitude), -90, 90);
    Guard.Against.OutOfRange(longitude, nameof(longitude), -180, 180);
    Guard.Against.NegativeOrZero(routeId);

    return new Stop(new StopName(name), new Location(latitude, longitude), new RouteId(routeId));
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
