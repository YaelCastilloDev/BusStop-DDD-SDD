using BusStop.Core.CommentAggregate;

namespace BusStop.UnitTests.Core.CommentAggregate;

public class CommentContentTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidValue()
    {
        var content = new CommentContent("Great stop!");

        content.Value.ShouldBe("Great stop!");
    }

    [Fact]
    public void Constructor_Throws_WhenNull()
    {
        Should.Throw<ArgumentException>(() => new CommentContent(null!));
    }

    [Fact]
    public void Constructor_Throws_WhenEmpty()
    {
        Should.Throw<ArgumentException>(() => new CommentContent(""));
    }

    [Fact]
    public void Constructor_Throws_WhenWhitespace()
    {
        Should.Throw<ArgumentException>(() => new CommentContent("   "));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var content = CommentContent.From("Great stop!");

        content.Value.ShouldBe("Great stop!");
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new CommentContent("Great stop!");
        var b = new CommentContent("Great stop!");

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new CommentContent("Great stop!");
        var b = new CommentContent("Bad stop!");

        a.ShouldNotBe(b);
    }
}
