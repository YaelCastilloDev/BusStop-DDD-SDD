using Ardalis.SharedKernel;

namespace BusStop.Core.RouteAggregate;

public sealed class RouteId : ValueObject
{
    public long Value { get; }

    public RouteId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
