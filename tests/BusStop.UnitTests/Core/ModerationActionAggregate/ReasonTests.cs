using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UnitTests.Core.ModerationActionAggregate;

// SPEC-TransitCatalog-ModerationAction
public class ReasonTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidValue()
    {
        var reason = new Reason("Hate speech detected");

        reason.Value.ShouldBe("Hate speech detected");
    }

    [Fact]
    public void Constructor_Throws_WhenNull()
    {
        Should.Throw<ArgumentException>(() => new Reason(null!));
    }

    [Fact]
    public void Constructor_Throws_WhenEmpty()
    {
        Should.Throw<ArgumentException>(() => new Reason(""));
    }

    [Fact]
    public void Constructor_Throws_WhenWhitespace()
    {
        Should.Throw<ArgumentException>(() => new Reason("   "));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var reason = Reason.From("Hate speech detected");

        reason.Value.ShouldBe("Hate speech detected");
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new Reason("Hate speech detected");
        var b = new Reason("Hate speech detected");

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new Reason("Hate speech detected");
        var b = new Reason("Spam content");

        a.ShouldNotBe(b);
    }
}
