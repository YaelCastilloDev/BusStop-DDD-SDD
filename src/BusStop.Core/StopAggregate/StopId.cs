using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class StopId : ValueObject
{
    public long Value { get; }

    public StopId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
