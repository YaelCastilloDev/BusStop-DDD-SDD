using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UnitTests.Core.ModerationActionAggregate;

// SPEC-TransitCatalog-ModerationAction
public class ModerationActionIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new ModerationActionId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new ModerationActionId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new ModerationActionId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new ModerationActionId(5);
        var b = new ModerationActionId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new ModerationActionId(5);
        var b = new ModerationActionId(10);

        a.ShouldNotBe(b);
    }
}
