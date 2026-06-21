using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class StopName(string value) : ValueObject
{
  public string Value { get; } = Guard.Against.NullOrWhiteSpace(value);

  public static StopName From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
