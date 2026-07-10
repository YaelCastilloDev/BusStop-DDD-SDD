using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.UserAggregate;

public class UserIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new UserId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new UserId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new UserId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new UserId(5);
        var b = new UserId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new UserId(5);
        var b = new UserId(10);

        a.ShouldNotBe(b);
    }
}
