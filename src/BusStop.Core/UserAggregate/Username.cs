using Ardalis.SharedKernel;

namespace BusStop.Core.UserAggregate;

public sealed class Username(string value) : ValueObject
{
  public string Value { get; } = Guard.Against.NullOrWhiteSpace(value);

  public static Username From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
