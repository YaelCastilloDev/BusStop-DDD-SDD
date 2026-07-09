using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteId(long value) : ValueObject
{
  public long Value { get; } = value > 0
    ? value
    : throw new DomainValidationException("RouteId must be positive.", nameof(value));

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
