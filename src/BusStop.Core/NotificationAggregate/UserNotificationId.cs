using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace BusStop.Core.NotificationAggregate;

public sealed class UserNotificationId(long value) : ValueObject
{
  public long Value { get; } = Guard.Against.NegativeOrZero(value);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
