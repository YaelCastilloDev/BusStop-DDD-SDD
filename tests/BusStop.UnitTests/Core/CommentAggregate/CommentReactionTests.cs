using BusStop.Core.CommentAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.CommentAggregate;

public class CommentReactionTests
{
    [Fact]
    public void From_CreatesInstance_WhenValidInput()
    {
        var reaction = CommentReaction.From(new UserId(1), true);

        reaction.UserId.Value.ShouldBe(1);
        reaction.IsLike.ShouldBeTrue();
    }

    [Fact]
    public void From_Throws_WhenNullUserId()
    {
        Should.Throw<ArgumentNullException>(() => CommentReaction.From(null!, true));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenSameUserIdDifferentIsLike()
    {
        var a = CommentReaction.From(new UserId(1), true);
        var b = CommentReaction.From(new UserId(1), false);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameUserIdAndSameIsLike()
    {
        var a = CommentReaction.From(new UserId(1), true);
        var b = CommentReaction.From(new UserId(1), true);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentUserId()
    {
        var a = CommentReaction.From(new UserId(1), true);
        var b = CommentReaction.From(new UserId(2), true);

        a.ShouldNotBe(b);
    }
}
