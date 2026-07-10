using BusStop.Core.RouteAggregate;

namespace BusStop.UnitTests.Core.RouteAggregate;

public class RouteNameTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidValue()
    {
        var name = new RouteName("Line A");

        name.Value.ShouldBe("Line A");
    }

    [Fact]
    public void Constructor_Throws_WhenNull()
    {
        Should.Throw<ArgumentException>(() => new RouteName(null!));
    }

    [Fact]
    public void Constructor_Throws_WhenEmpty()
    {
        Should.Throw<ArgumentException>(() => new RouteName(""));
    }

    [Fact]
    public void Constructor_Throws_WhenWhitespace()
    {
        Should.Throw<ArgumentException>(() => new RouteName("   "));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var name = RouteName.From("Line A");

        name.Value.ShouldBe("Line A");
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new RouteName("Line A");
        var b = new RouteName("Line A");

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new RouteName("Line A");
        var b = new RouteName("Line B");

        a.ShouldNotBe(b);
    }
}
