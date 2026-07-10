using BusStop.Core.NotificationAggregate;

namespace BusStop.UnitTests.Core.NotificationAggregate;

public class UserNotificationIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new UserNotificationId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new UserNotificationId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new UserNotificationId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new UserNotificationId(5);
        var b = new UserNotificationId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new UserNotificationId(5);
        var b = new UserNotificationId(10);

        a.ShouldNotBe(b);
    }
}
