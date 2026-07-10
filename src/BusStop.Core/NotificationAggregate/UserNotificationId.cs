using Ardalis.SharedKernel;

namespace BusStop.Core.NotificationAggregate;

public sealed class UserNotificationId : ValueObject
{
    public long Value { get; }

    public UserNotificationId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
