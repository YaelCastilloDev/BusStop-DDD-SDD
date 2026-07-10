using Ardalis.SharedKernel;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteName : ValueObject
{
    public string Value { get; }

    public RouteName(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static RouteName From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
