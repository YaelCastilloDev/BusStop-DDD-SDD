using Ardalis.SharedKernel;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteName(string value) : ValueObject
{
  public string Value { get; } = Guard.Against.NullOrWhiteSpace(value);

  public static RouteName From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
