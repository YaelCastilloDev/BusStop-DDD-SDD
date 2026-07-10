using Ardalis.SharedKernel;

namespace BusStop.Core.CountryAggregate;

public sealed class CountryId : ValueObject
{
    public long Value { get; }

    public CountryId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    public static CountryId From(long value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
