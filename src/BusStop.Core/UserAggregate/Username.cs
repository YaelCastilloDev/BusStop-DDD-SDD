using Ardalis.SharedKernel;

namespace BusStop.Core.UserAggregate;

public sealed class Username : ValueObject
{
    public string Value { get; }

    public Username(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static Username From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
