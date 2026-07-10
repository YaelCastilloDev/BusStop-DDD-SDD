using BusStop.Core.Errors;
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
        Guard.Against.Null(name, nameof(name));
        Guard.Against.Null(createdById, nameof(createdById));

        Name = name;
        CreatedById = createdById;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Route> Create(string name, long createdById)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(RouteErrors.EmptyName);
        if (createdById <= 0)
            errors.Add(RouteErrors.InvalidCreatedBy);

        if (errors.Count > 0)
            return Result<Route>.Error(new ErrorList(errors));

        return Result<Route>.Success(new Route(new RouteName(name), new UserId(createdById)));
    }

    public void UpdateName(RouteName newName)
    {
        Guard.Against.Null(newName, nameof(newName));
        Name = newName;
    }

    public Result Delete(UserId deletedBy)
    {
        Guard.Against.Null(deletedBy, nameof(deletedBy));

        if (IsDeleted)
            return Result.Error(new ErrorList([RouteErrors.AlreadyDeleted]));

        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy.Value;
        RegisterDomainEvent(new RouteDeletedEvent(Id));
        return Result.Success();
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
