namespace BusStop.Core.RouteAggregate.Events;

public sealed class RouteDeletedEvent(long routeId, long deletedByUserId) : DomainEventBase
{
  public long RouteId { get; } = routeId;
  public long DeletedByUserId { get; } = deletedByUserId;
}
