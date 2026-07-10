using Ardalis.SharedKernel;

namespace BusStop.Core.UserAggregate;

public sealed class UserId : ValueObject
{
    public long Value { get; }

    public UserId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
