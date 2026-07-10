using BusStop.Core.RouteAggregate;

namespace BusStop.UnitTests.Core.RouteAggregate;

public class RouteIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new RouteId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new RouteId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new RouteId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new RouteId(5);
        var b = new RouteId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new RouteId(5);
        var b = new RouteId(10);

        a.ShouldNotBe(b);
    }
}
