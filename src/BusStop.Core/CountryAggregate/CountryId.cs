using Ardalis.SharedKernel;

namespace BusStop.Core.CountryAggregate;

public sealed class CountryId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  public static CountryId From(long value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
