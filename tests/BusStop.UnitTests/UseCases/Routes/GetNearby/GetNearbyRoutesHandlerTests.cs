using BusStop.UseCases.Routes.GetNearby;
using NSubstitute;
using Shouldly;
using Xunit;

namespace BusStop.UnitTests.UseCases.Routes.GetNearby;

public class GetNearbyRoutesHandlerTests
{
    private readonly INearbyRoutesQueryService _queryService;
    private readonly GetNearbyRoutesHandler _handler;

    public GetNearbyRoutesHandlerTests()
    {
        _queryService = Substitute.For<INearbyRoutesQueryService>();
        _handler = new GetNearbyRoutesHandler(_queryService);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessResult_WithQueryServiceResult()
    {
        // Arrange
        var query = new GetNearbyRoutesQuery(10.0, 20.0, 0.5, 15.0);
        var expectedResult = new NearbyRoutesResult(
            new List<NearbyRouteDto> { new(1, "Route 1", 1, DateTime.UtcNow, false, 100) },
            false,
            "Found 1 route(s) within 500 meters.");

        _queryService.GetNearbyRoutesAsync(query.Latitude, query.Longitude, query.InitialRadiusKm, query.FallbackRadiusKm, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        await _queryService.Received(1).GetNearbyRoutesAsync(query.Latitude, query.Longitude, query.InitialRadiusKm, query.FallbackRadiusKm, Arg.Any<CancellationToken>());
    }
}
