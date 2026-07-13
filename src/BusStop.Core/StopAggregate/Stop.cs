using BusStop.Core.Errors;
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
        Guard.Against.Null(name, nameof(name));
        Guard.Against.Null(location, nameof(location));
        Guard.Against.Null(routeId, nameof(routeId));

        Name = name;
        Location = location;
        RouteId = routeId;
    }

    public static Result<Stop> Create(string name, double latitude, double longitude, long routeId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(StopErrors.EmptyName);
        if (latitude < -90 || latitude > 90)
            errors.Add(StopErrors.InvalidLatitude);
        if (longitude < -180 || longitude > 180)
            errors.Add(StopErrors.InvalidLongitude);
        if (routeId <= 0)
            errors.Add(StopErrors.InvalidRouteId);

        if (errors.Count > 0)
            return Result<Stop>.Error(new ErrorList(errors));

        return Result<Stop>.Success(new Stop(new StopName(name), new Location(latitude, longitude), new RouteId(routeId)));
    }

    public Result UpdateName(StopName newName)
    {
        Guard.Against.Null(newName, nameof(newName));
        Name = newName;
        return Result.Success();
    }

    public Result UpdateLocation(Location newLocation)
    {
        Guard.Against.Null(newLocation, nameof(newLocation));
        Location = newLocation;
        return Result.Success();
    }

    public Result Delete(UserId deletedBy)
    {
        Guard.Against.Null(deletedBy, nameof(deletedBy));

        if (IsDeleted)
            return Result.Error(new ErrorList([StopErrors.AlreadyDeleted]));

        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy.Value;
        RegisterDomainEvent(new StopDeletedEvent(Id));
        return Result.Success();
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
