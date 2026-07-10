using BusStop.Core.StopAggregate;

namespace BusStop.UnitTests.Core.StopAggregate;

public class StopNameTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidValue()
    {
        var name = new StopName("Main Street");

        name.Value.ShouldBe("Main Street");
    }

    [Fact]
    public void Constructor_Throws_WhenNull()
    {
        Should.Throw<ArgumentException>(() => new StopName(null!));
    }

    [Fact]
    public void Constructor_Throws_WhenEmpty()
    {
        Should.Throw<ArgumentException>(() => new StopName(""));
    }

    [Fact]
    public void Constructor_Throws_WhenWhitespace()
    {
        Should.Throw<ArgumentException>(() => new StopName("   "));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var name = StopName.From("Main Street");

        name.Value.ShouldBe("Main Street");
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new StopName("Main Street");
        var b = new StopName("Main Street");

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new StopName("Main Street");
        var b = new StopName("Broadway");

        a.ShouldNotBe(b);
    }
}
