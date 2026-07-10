using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.StopAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.UnitTests.Core.StopAggregate;

public class StopTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = Stop.Create("Main Street", 45.5, -122.6, 1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe("Main Street");
        result.Value.Location.Latitude.ShouldBe(45.5);
        result.Value.Location.Longitude.ShouldBe(-122.6);
        result.Value.RouteId.Value.ShouldBe(1);
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyName()
    {
        var result = Stop.Create("", 45.5, -122.6, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.EmptyName));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceName()
    {
        var result = Stop.Create("   ", 45.5, -122.6, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.EmptyName));
    }

    [Fact]
    public void Create_ReturnsError_WhenLatitudeBelowMinus90()
    {
        var result = Stop.Create("Main Street", -90.1, -122.6, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLatitude));
    }

    [Fact]
    public void Create_ReturnsError_WhenLatitudeAbove90()
    {
        var result = Stop.Create("Main Street", 90.1, -122.6, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLatitude));
    }

    [Fact]
    public void Create_ReturnsError_WhenLongitudeBelowMinus180()
    {
        var result = Stop.Create("Main Street", 45.5, -180.1, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLongitude));
    }

    [Fact]
    public void Create_ReturnsError_WhenLongitudeAbove180()
    {
        var result = Stop.Create("Main Street", 45.5, 180.1, 1);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLongitude));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenInvalidRouteId(long routeId)
    {
        var result = Stop.Create("Main Street", 45.5, -122.6, routeId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidRouteId));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = Stop.Create("", -91, -181, 0);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.EmptyName));
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLatitude));
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidLongitude));
        result.Errors.ShouldContain(e => e.Contains(StopErrors.InvalidRouteId));
    }

    [Fact]
    public void Delete_Succeeds_WhenNotDeleted()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;

        var result = stop.Delete(new UserId(1));

        result.IsSuccess.ShouldBeTrue();
        stop.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_ReturnsError_WhenAlreadyDeleted()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;
        stop.Delete(new UserId(1));

        var result = stop.Delete(new UserId(2));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(StopErrors.AlreadyDeleted));
    }

    [Fact]
    public void Delete_Throws_WhenNullDeletedBy()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;

        Should.Throw<ArgumentNullException>(() => stop.Delete(null!));
    }

    [Fact]
    public void IsDeleted_ReturnsTrue_AfterDelete()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;

        stop.IsDeleted.ShouldBeFalse();
        stop.Delete(new UserId(1));
        stop.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void UpdateName_Throws_WhenNullName()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;

        Should.Throw<ArgumentNullException>(() => stop.UpdateName(null!));
    }

    [Fact]
    public void UpdateLocation_Throws_WhenNullLocation()
    {
        var stopResult = Stop.Create("Main Street", 45.5, -122.6, 1);
        var stop = stopResult.Value;

        Should.Throw<ArgumentNullException>(() => stop.UpdateLocation(null!));
    }
}
