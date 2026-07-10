using BusStop.Core.CommentAggregate;

namespace BusStop.UnitTests.Core.CommentAggregate;

public class CommentIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new CommentId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new CommentId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new CommentId(-1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new CommentId(5);
        var b = new CommentId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new CommentId(5);
        var b = new CommentId(10);

        a.ShouldNotBe(b);
    }
}
