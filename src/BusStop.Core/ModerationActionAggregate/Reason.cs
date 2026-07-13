namespace BusStop.Core.ModerationActionAggregate;

public sealed class Reason : ValueObject
{
    public string Value { get; }

    public Reason(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static Reason From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
