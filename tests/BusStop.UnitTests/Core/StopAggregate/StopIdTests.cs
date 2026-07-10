using BusStop.Core.StopAggregate;

namespace BusStop.UnitTests.Core.StopAggregate;

public class StopIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new StopId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new StopId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new StopId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new StopId(5);
        var b = new StopId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new StopId(5);
        var b = new StopId(10);

        a.ShouldNotBe(b);
    }
}
