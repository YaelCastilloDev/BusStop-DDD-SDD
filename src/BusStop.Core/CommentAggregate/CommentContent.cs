using Ardalis.SharedKernel;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentContent : ValueObject
{
    public string Value { get; }

    public CommentContent(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static CommentContent From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
