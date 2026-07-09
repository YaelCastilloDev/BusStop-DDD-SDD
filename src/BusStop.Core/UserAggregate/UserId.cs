using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.UserAggregate;

public sealed class UserId(long value) : ValueObject
{
  public long Value { get; } = value > 0
    ? value
    : throw new DomainValidationException("UserId must be positive.", nameof(value));

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
