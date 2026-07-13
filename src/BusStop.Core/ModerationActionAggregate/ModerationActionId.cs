namespace BusStop.Core.ModerationActionAggregate;

public sealed class ModerationActionId : ValueObject
{
    public long Value { get; }

    public ModerationActionId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
