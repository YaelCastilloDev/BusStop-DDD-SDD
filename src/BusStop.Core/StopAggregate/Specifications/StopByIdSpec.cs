namespace BusStop.Core.StopAggregate.Specifications;

public sealed class StopByIdSpec : Specification<Stop>
{
  public StopByIdSpec(StopId stopId) =>
    Query.Where(s => s.Id == stopId.Value);
}
