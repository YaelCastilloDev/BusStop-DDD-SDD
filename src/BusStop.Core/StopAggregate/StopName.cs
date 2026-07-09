using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.StopAggregate;

public sealed class StopName(string value) : ValueObject
{
  public string Value { get; } = !string.IsNullOrWhiteSpace(value)
    ? value
    : throw new DomainValidationException("StopName is required.", nameof(value));

  public static StopName From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
