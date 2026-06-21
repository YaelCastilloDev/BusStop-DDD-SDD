using Ardalis.SharedKernel;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
