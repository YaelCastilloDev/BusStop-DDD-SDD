using BusStop.Core.CommentAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.CommentAggregate;

public class CommentReactionTests
{
    [Fact]
    public void From_CreatesInstance_WhenValidInput()
    {
        var reaction = CommentReaction.From(new UserId(1), ReactionType.Like);

        reaction.UserId.Value.ShouldBe(1);
        reaction.ReactionType.ShouldBe(ReactionType.Like);
    }

    [Fact]
    public void From_Throws_WhenNullUserId()
    {
        Should.Throw<ArgumentNullException>(() => CommentReaction.From(null!, ReactionType.Like));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameUserId()
    {
        var a = CommentReaction.From(new UserId(1), ReactionType.Like);
        var b = CommentReaction.From(new UserId(1), ReactionType.Dislike);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentUserId()
    {
        var a = CommentReaction.From(new UserId(1), ReactionType.Like);
        var b = CommentReaction.From(new UserId(2), ReactionType.Like);

        a.ShouldNotBe(b);
    }
}
