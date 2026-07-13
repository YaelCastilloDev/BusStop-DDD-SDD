namespace BusStop.Core.RouteAggregate.Events;

public sealed class RouteModeratedEvent(long routeId, long moderatorUserId) : DomainEventBase
{
    public long RouteId { get; } = routeId;
    public long ModeratorUserId { get; } = moderatorUserId;
}
