namespace BusStop.Core.RouteAggregate.Events;

public sealed class RouteDeletedEvent(long routeId) : DomainEventBase
{
  public long RouteId { get; } = routeId;
}
