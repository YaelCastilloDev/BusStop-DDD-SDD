using Ardalis.Result;
using BusStop.Core.CommentAggregate;
using BusStop.Core.Errors;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.CommentAggregate;

public class CommentTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = Comment.Create("Great stop!", 1, 10);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Content.Value.ShouldBe("Great stop!");
        result.Value.UserId.Value.ShouldBe(1);
        result.Value.RouteId.Value.ShouldBe(10);
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyContent()
    {
        var result = Comment.Create("", 1, 10);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.EmptyContent));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceContent()
    {
        var result = Comment.Create("   ", 1, 10);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.EmptyContent));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenInvalidUserId(long userId)
    {
        var result = Comment.Create("Great stop!", userId, 10);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.InvalidUser));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenInvalidRouteId(long routeId)
    {
        var result = Comment.Create("Great stop!", 1, routeId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.InvalidRoute));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = Comment.Create("", 0, 0);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.EmptyContent));
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.InvalidUser));
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.InvalidRoute));
    }

    [Fact]
    public void Moderate_Succeeds_WhenNotModerated()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;

        var result = comment.Moderate(new UserId(5));

        result.IsSuccess.ShouldBeTrue();
        comment.IsModerated.ShouldBeTrue();
    }

    [Fact]
    public void Moderate_ReturnsError_WhenAlreadyModerated()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;
        comment.Moderate(new UserId(5));

        var result = comment.Moderate(new UserId(6));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(CommentErrors.AlreadyModerated));
    }

    [Fact]
    public void Moderate_Throws_WhenNullModeratedBy()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;

        Should.Throw<ArgumentNullException>(() => comment.Moderate(null!));
    }

    [Fact]
    public void IsModerated_ReturnsTrue_AfterModerate()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;

        comment.IsModerated.ShouldBeFalse();
        comment.Moderate(new UserId(5));
        comment.IsModerated.ShouldBeTrue();
    }

    [Fact]
    public void AddReaction_AddsReaction_WhenNoPriorReaction()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;

        var reactionResult = comment.AddReaction(new UserId(1), ReactionType.Like);
        reactionResult.IsSuccess.ShouldBeTrue();

        comment.Reactions.ShouldHaveSingleItem();
        comment.Reactions.Single().UserId.Value.ShouldBe(1);
        comment.Reactions.Single().ReactionType.ShouldBe(ReactionType.Like);
    }

    [Fact]
    public void AddReaction_ReplacesReaction_WhenSameUserReacted()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;
        var reactionResult1 = comment.AddReaction(new UserId(1), ReactionType.Like);
        reactionResult1.IsSuccess.ShouldBeTrue();

        var reactionResult2 = comment.AddReaction(new UserId(1), ReactionType.Dislike);
        reactionResult2.IsSuccess.ShouldBeTrue();

        comment.Reactions.ShouldHaveSingleItem();
        comment.Reactions.Single().ReactionType.ShouldBe(ReactionType.Dislike);
    }

    [Fact]
    public void AddReaction_Throws_WhenNullUserId()
    {
        var commentResult = Comment.Create("Great stop!", 1, 10);
        var comment = commentResult.Value;

        Should.Throw<ArgumentNullException>(() => comment.AddReaction(null!, ReactionType.Like));
    }
}
