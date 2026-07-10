using BusStop.Core.CountryAggregate;

namespace BusStop.UnitTests.Core.CountryAggregate;

public class CountryIdTests
{
    [Fact]
    public void Constructor_Succeeds_WhenPositiveValue()
    {
        var id = new CountryId(1);

        id.Value.ShouldBe(1);
    }

    [Fact]
    public void Constructor_Throws_WhenZero()
    {
        Should.Throw<ArgumentException>(() => new CountryId(0));
    }

    [Fact]
    public void Constructor_Throws_WhenNegative()
    {
        Should.Throw<ArgumentException>(() => new CountryId(-1));
    }

    [Fact]
    public void From_CreatesInstance_WithSameValue()
    {
        var id = CountryId.From(5);

        id.Value.ShouldBe(5);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameValue()
    {
        var a = new CountryId(5);
        var b = new CountryId(5);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValue()
    {
        var a = new CountryId(5);
        var b = new CountryId(10);

        a.ShouldNotBe(b);
    }
}
