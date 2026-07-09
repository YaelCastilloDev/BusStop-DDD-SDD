using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteName(string value) : ValueObject
{
  public string Value { get; } = !string.IsNullOrWhiteSpace(value)
    ? value
    : throw new DomainValidationException("RouteName is required.", nameof(value));

  public static RouteName From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
