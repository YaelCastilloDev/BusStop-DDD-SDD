using BusStop.Web.Comments;

namespace BusStop.UnitTests.Web.Comments;

public sealed class ReactToCommentValidatorTests
{
    private readonly ReactToCommentValidator _validator;

    public ReactToCommentValidatorTests()
    {
        _validator = new ReactToCommentValidator();
    }

    [Fact]
    public async Task ShouldAccept_True()
    {
        var request = new ReactToCommentRequest(1, true);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAccept_False()
    {
        var request = new ReactToCommentRequest(1, false);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReject_InvalidCommentId()
    {
        var request = new ReactToCommentRequest(0, true);
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
        result.IsValid.ShouldBeFalse("CommentId must be greater than 0");
    }
}
