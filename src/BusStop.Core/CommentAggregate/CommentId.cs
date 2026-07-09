using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentId(long value) : ValueObject
{
  public long Value { get; } = value > 0
    ? value
    : throw new DomainValidationException("CommentId must be positive.", nameof(value));

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
