using Ardalis.Result;

namespace BusStop.UnitTests.Web.Auth;

public sealed class RegisterResultTests
{
    [Fact]
    public void FailedResult_ValueAccess_ReturnsNull()
    {
        var result = Result<UserResponse>.Error("msg");

        result.Value.ShouldBeNull();
    }

    [Fact]
    public void FailedResult_ValueIdAccess_ThrowsNRE()
    {
        var result = Result<UserResponse>.Error("msg");

        Should.Throw<NullReferenceException>(() =>
        {
            var x = new { result.Value.Id };
        });
    }

    private sealed record UserResponse(long Id, string Name);
}
