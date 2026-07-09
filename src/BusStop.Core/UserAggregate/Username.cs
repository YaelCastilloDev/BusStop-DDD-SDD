using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.UserAggregate;

public sealed class Username(string value) : ValueObject
{
  public string Value { get; } = !string.IsNullOrWhiteSpace(value)
    ? value
    : throw new DomainValidationException("Username is required.", nameof(value));

  public static Username From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
