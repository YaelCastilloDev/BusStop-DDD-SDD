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
    Guard.Against.NullOrWhiteSpace(name);
    Guard.Against.NegativeOrZero(createdById);

    return new Route(new RouteName(name), new UserId(createdById));
  }

  public void UpdateName(RouteName newName)
  {
    Guard.Against.Null(newName);
    Name = newName;
  }

  public void Delete(UserId deletedBy)
  {
    Guard.Against.Null(deletedBy);
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy.Value;
    RegisterDomainEvent(new RouteDeletedEvent(Id));
  }

  public bool IsDeleted => DeletedAt.HasValue;
}
