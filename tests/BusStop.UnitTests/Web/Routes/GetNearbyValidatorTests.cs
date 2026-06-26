using BusStop.Web.Routes;
using Shouldly;
using Xunit;

namespace BusStop.UnitTests.Web.Routes;

public class GetNearbyValidatorTests
{
    private readonly GetNearbyValidator _validator;

    public GetNearbyValidatorTests()
    {
        _validator = new GetNearbyValidator();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(45.5, -122.6)]
    public async Task Validate_ValidCoordinates_ShouldNotHaveError(double latitude, double longitude)
    {
        var request = new GetNearbyRequest(latitude, longitude);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-90.1, 0)]
    [InlineData(90.1, 0)]
    public async Task Validate_InvalidLatitude_ShouldHaveError(double latitude, double longitude)
    {
        var request = new GetNearbyRequest(latitude, longitude);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(request.Latitude));
    }

    [Theory]
    [InlineData(0, -180.1)]
    [InlineData(0, 180.1)]
    public async Task Validate_InvalidLongitude_ShouldHaveError(double latitude, double longitude)
    {
        var request = new GetNearbyRequest(latitude, longitude);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(request.Longitude));
    }
}
