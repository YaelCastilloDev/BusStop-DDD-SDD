using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentContent(string value) : ValueObject
{
  public string Value { get; } = !string.IsNullOrWhiteSpace(value)
    ? value
    : throw new DomainValidationException("CommentContent is required.", nameof(value));

  public static CommentContent From(string value) => new(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
