using BusStop.Core.StopAggregate;

namespace BusStop.UnitTests.Core.StopAggregate;

public class LocationTests
{
    [Fact]
    public void Constructor_Succeeds_WhenValidCoordinates()
    {
        var location = new Location(45.5, -122.6);

        location.Latitude.ShouldBe(45.5);
        location.Longitude.ShouldBe(-122.6);
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public void Constructor_Succeeds_AtBoundaryLatitudes(double latitude)
    {
        var location = new Location(latitude, 0);

        location.Latitude.ShouldBe(latitude);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public void Constructor_Succeeds_AtBoundaryLongitudes(double longitude)
    {
        var location = new Location(0, longitude);

        location.Longitude.ShouldBe(longitude);
    }

    [Fact]
    public void Constructor_Throws_WhenLatitudeBelowMinus90()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Location(-90.1, 0));
    }

    [Fact]
    public void Constructor_Throws_WhenLatitudeAbove90()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Location(90.1, 0));
    }

    [Fact]
    public void Constructor_Throws_WhenLongitudeBelowMinus180()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Location(0, -180.1));
    }

    [Fact]
    public void Constructor_Throws_WhenLongitudeAbove180()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Location(0, 180.1));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameCoordinates()
    {
        var a = new Location(45.5, -122.6);
        var b = new Location(45.5, -122.6);

        a.ShouldBe(b);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentCoordinates()
    {
        var a = new Location(45.5, -122.6);
        var b = new Location(40.7, -74.0);

        a.ShouldNotBe(b);
    }
}
