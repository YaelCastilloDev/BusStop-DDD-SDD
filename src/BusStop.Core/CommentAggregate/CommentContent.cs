using Ardalis.SharedKernel;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentContent(string value) : ValueObject
{
  public string Value { get; } = Guard.Against.NullOrWhiteSpace(value);

  public static CommentContent From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
