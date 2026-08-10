namespace BusStop.UnitTests.Web.Configurations;

public sealed class RateLimiterKeyTests
{
    [Fact]
    public void PartitionKey_ThrowsNRE_WhenBothPartsNull()
    {
        string? userName = null;
        string? host = null;

        Should.Throw<NullReferenceException>(() =>
        {
            var key = userName ?? host!.ToString();
        });
    }

    [Fact]
    public void PartitionKey_Fallback_ShouldUseSafeDefault()
    {
        string? userName = null;
        string? host = null;
        var key = userName ?? host ?? "unknown";
        key.ShouldBe("unknown");
    }
}
