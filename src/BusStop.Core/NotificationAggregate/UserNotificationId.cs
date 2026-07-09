using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.NotificationAggregate;

public sealed class UserNotificationId(long value) : ValueObject
{
  public long Value { get; } = value > 0
    ? value
    : throw new DomainValidationException("UserNotificationId must be positive.", nameof(value));

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
