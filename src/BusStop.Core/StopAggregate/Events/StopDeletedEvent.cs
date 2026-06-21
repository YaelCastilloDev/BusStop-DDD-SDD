namespace BusStop.Core.StopAggregate.Events;

public sealed class StopDeletedEvent(long stopId) : DomainEventBase
{
  public long StopId { get; } = stopId;
}
