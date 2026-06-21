using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class StopId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
