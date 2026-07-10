using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class StopName : ValueObject
{
    public string Value { get; }

    public StopName(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static StopName From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
