using BusStop.Core.Exceptions;
using BusStop.Core.RouteAggregate.Events;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.RouteAggregate;

public class Route : EntityBase<long>, IAggregateRoot
{
  public RouteName Name { get; private set; }
  public UserId CreatedById { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? DeletedAt { get; private set; }
  public long? DeletedBy { get; private set; }

#pragma warning disable CS8618
  private Route() { }
#pragma warning restore CS8618

  private Route(RouteName name, UserId createdById)
  {
    Name = name;
    CreatedById = createdById;
    CreatedAt = DateTime.UtcNow;
  }

  public static Route Create(string name, long createdById)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new DomainValidationException("Route name is required.", nameof(name));
    if (createdById <= 0)
      throw new DomainValidationException("CreatedById must be positive.", nameof(createdById));

    return new Route(new RouteName(name), new UserId(createdById));
  }

  public void UpdateName(RouteName newName)
  {
    if (newName is null)
      throw new DomainValidationException("New name is required.", nameof(newName));
    Name = newName;
  }

  public void Delete(UserId deletedBy)
  {
    if (deletedBy is null)
      throw new DomainValidationException("DeletedBy is required.", nameof(deletedBy));
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy.Value;
    RegisterDomainEvent(new RouteDeletedEvent(Id));
  }

  public bool IsDeleted => DeletedAt.HasValue;
}
