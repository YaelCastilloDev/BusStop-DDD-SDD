using Ardalis.SharedKernel;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
