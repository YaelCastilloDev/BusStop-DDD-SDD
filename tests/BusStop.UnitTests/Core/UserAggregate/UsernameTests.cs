using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.UserAggregate;

public class UsernameTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidValue()
    {
        var username = new Username("john_doe");

        username.Value.ShouldBe("john_doe");
    }

    [Fact]
    public void Constructor_Throws_WhenNull()
    {
        Should.Throw<ArgumentException>(() => new Username(null!));
    }

    [Fact]
    public void Constructor_Throws_WhenEmpty()
    {
        Should.Throw<ArgumentException>(() => new Username(""));
    }

    [Fact]
    public void Constructor_Throws_WhenWhitespace()
    {
        Should.Throw<ArgumentException>(() => new Username("   "));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var username = Username.From("john_doe");

        username.Value.ShouldBe("john_doe");
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new Username("john_doe");
        var b = new Username("john_doe");

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new Username("john_doe");
        var b = new Username("jane_doe");

        a.ShouldNotBe(b);
    }
}
