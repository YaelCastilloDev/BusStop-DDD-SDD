using Ardalis.SharedKernel;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentId : ValueObject
{
    public long Value { get; }

    public CommentId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
