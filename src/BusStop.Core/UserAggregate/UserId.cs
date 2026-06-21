using Ardalis.SharedKernel;

namespace BusStop.Core.UserAggregate;

public sealed class UserId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
