using Ardalis.Result;

namespace BusStop.UnitTests.Web.Comments;

/// <summary>
/// Regression tests for the Comments Create endpoint's NRE risk.
/// <see cref="BusStop.Web.Comments.Create.HandleAsync"/> accesses
/// <c>result.Value.Id</c> without a null-conditional on line 22 of Create.cs.
/// <see cref="Result{T}.Value"/> returns <c>default(T)</c> (null for reference types)
/// when the result is a failure. Accessing <c>.Id</c> on <c>null</c> causes a
/// <see cref="NullReferenceException"/>.
/// </summary>
public sealed class CreateEndpointTests
{
    /// <summary>
    /// BUG: In Create.cs line 22, <c>new { result.Value.Id }</c> is evaluated
    /// BEFORE the result is checked for success inside ToCreatedResultAsync.
    /// When the result is a failure, <c>result.Value</c> returns <c>null</c>
    /// (the default for <see cref="CommentDto"/> which is a reference type).
    /// Then <c>.Id</c> on <c>null</c> causes a <see cref="NullReferenceException"/>.
    ///
    /// This test demonstrates: Result&lt;T&gt;.Value returns default(T) on failed results,
    /// and accessing members on that null value throws NRE.
    /// </summary>
    [Fact]
    public void ResultValue_ReturnsNull_OnFailedResult_CausingNRE_OnMemberAccess()
    {
        // Arrange: create a failed result
        var failedResult = Result<CommentDto>.Error("Validation failed");

        // Act: .Value returns null (default for reference type) on a failed result
        var value = failedResult.Value;

        // Assert: Value is null on a failed result
        value.ShouldBeNull(
            "Result<T>.Value returns default(T) on failed results");

        // This is the NRE bug in Create.cs: accessing .Id on null
        Should.Throw<NullReferenceException>(() =>
        {
            var _ = failedResult.Value!.Id;
        }, "accessing .Id on null Value causes NRE — this is the bug in Comments/Create.cs line 22");

        // The fix: use null-conditional (result.Value?.Id)
        // or check IsSuccess before accessing Value
        if (failedResult.IsSuccess)
        {
            // Safe pattern: Value is guaranteed non-null here
            var _ = failedResult.Value.Id;
        }
    }

    /// <summary>
    /// Safe pattern: null-conditional avoids the NRE.
    /// </summary>
    [Fact]
    public void NullConditional_DoesNotThrow_OnFailedResult()
    {
        var failedResult = Result<CommentDto>.Error("Validation failed");

        // Using null-conditional is safe — it returns null instead of throwing
        var id = failedResult.Value?.Id;

        id.ShouldBeNull("null-conditional access on failed result should return null, not throw");
    }

    private sealed record CommentDto(long Id, string Content, long RouteId);
}
