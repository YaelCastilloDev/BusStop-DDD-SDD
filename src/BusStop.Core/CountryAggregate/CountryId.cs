using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.CountryAggregate;

public sealed class CountryId(long value) : ValueObject
{
  public long Value { get; } = value > 0
    ? value
    : throw new DomainValidationException("CountryId must be positive.", nameof(value));

  public static CountryId From(long value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
