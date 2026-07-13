using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.ModerationActionAggregate;

namespace BusStop.UnitTests.Core.ModerationActionAggregate;

// SPEC-TransitCatalog-ModerationAction
public class ModerationActionTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenAllInputsValid()
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, ModerationCategory.HateSpeech, "Hate speech detected");

        result.IsSuccess.ShouldBeTrue();
        result.Value.TargetType.ShouldBe(TargetType.Comment);
        result.Value.TargetId.ShouldBe(10);
        result.Value.UserId.Value.ShouldBe(100);
        result.Value.IssuedBy.Value.ShouldBe(200);
        result.Value.Category.ShouldBe(ModerationCategory.HateSpeech);
        result.Value.Reason.Value.ShouldBe("Hate speech detected");
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenAllInputsValid_ForRouteTarget()
    {
        var result = ModerationAction.Create(TargetType.Route, 20, 300, 400, ModerationCategory.Spam, "Spam route");

        result.IsSuccess.ShouldBeTrue();
        result.Value.TargetType.ShouldBe(TargetType.Route);
        result.Value.Category.ShouldBe(ModerationCategory.Spam);
        result.Value.Reason.Value.ShouldBe("Spam route");
    }

    [Fact]
    public void Create_ReturnsError_WhenCategoryIsInvalid()
    {
        var invalidCategory = (ModerationCategory)0;
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, invalidCategory, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidCategory));
    }

    [Fact]
    public void Create_ReturnsError_WhenCategoryIsOutOfRange()
    {
        var invalidCategory = (ModerationCategory)7;
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, invalidCategory, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidCategory));
    }

    [Fact]
    public void Create_ReturnsError_WhenTargetTypeIsInvalid()
    {
        var invalidTarget = (TargetType)0;
        var result = ModerationAction.Create(invalidTarget, 10, 100, 200, ModerationCategory.Spam, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidTargetType));
    }

    [Fact]
    public void Create_ReturnsError_WhenTargetTypeIsOutOfRange()
    {
        var invalidTarget = (TargetType)3;
        var result = ModerationAction.Create(invalidTarget, 10, 100, 200, ModerationCategory.Spam, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidTargetType));
    }

    [Fact]
    public void Create_ReturnsError_WhenReasonIsNull()
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, ModerationCategory.Spam, null!);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.EmptyReason));
    }

    [Fact]
    public void Create_ReturnsError_WhenReasonIsEmpty()
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, ModerationCategory.Spam, "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.EmptyReason));
    }

    [Fact]
    public void Create_ReturnsError_WhenReasonIsWhitespace()
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, 200, ModerationCategory.Spam, "   ");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.EmptyReason));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenIssuedByIsInvalid(long issuedBy)
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, 100, issuedBy, ModerationCategory.Spam, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidIssuedBy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenTargetIdIsInvalid(long targetId)
    {
        var result = ModerationAction.Create(TargetType.Comment, targetId, 100, 200, ModerationCategory.Spam, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidTargetId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenUserIdIsInvalid(long userId)
    {
        var result = ModerationAction.Create(TargetType.Comment, 10, userId, 200, ModerationCategory.Spam, "Valid reason");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidUserId));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = ModerationAction.Create((TargetType)0, 0, 0, 0, (ModerationCategory)0, "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidTargetType));
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidTargetId));
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidUserId));
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidIssuedBy));
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.InvalidCategory));
        result.Errors.ShouldContain(e => e.Contains(ModerationActionErrors.EmptyReason));
    }
}
