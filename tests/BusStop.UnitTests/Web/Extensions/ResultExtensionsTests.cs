using Ardalis.Result;
using BusStop.Web.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace BusStop.UnitTests.Web.Extensions;

public sealed class ResultExtensionsTests
{
    [Fact]
    public async Task ToCreatedResultAsync_ShouldSetLocationHeader_OnSuccess()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Path = "/api/test";

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpContext.Returns(httpContext);

        var successResult = Result<TestDto>.Success(new TestDto(42, "test"));
        var routeValues = new { id = 42 };

        // Act
        await endpoint.ToCreatedResultAsync(successResult, routeValues, CancellationToken.None);

        // Assert
        httpContext.Response.StatusCode.ShouldBeOneOf([200, 201]);
        httpContext.Response.Headers.Location.ToString().ShouldBe("/api/test/42");
    }

    /// <summary>
    /// Verifies that ToCreatedResultAsync properly delegates to error handling
    /// when the result is a failure.
    /// </summary>
    [Fact]
    public async Task ToCreatedResultAsync_ShouldHandleError_OnFailedResult()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpContext.Returns(httpContext);
        // FastEndpoints IEndpoint has ValidationFailures property
        endpoint.ValidationFailures.Returns(new List<FluentValidation.Results.ValidationFailure>());

        var failedResult = Result<TestDto>.Error("Something went wrong");
        var routeValues = new { id = 0 };

        // Act
        await endpoint.ToCreatedResultAsync(failedResult, routeValues, CancellationToken.None);

        // Assert: Should not be 201, should be an error status (400 for generic error)
        // The HandleErrorResultAsync method sends errors for unrecognized statuses
        httpContext.Response.StatusCode.ShouldBeGreaterThanOrEqualTo(400,
            "failed result should produce an error status code");
    }

    /// <summary>
    /// Verifies that ToCreatedResultAsync returns 404 for NotFound results.
    /// </summary>
    [Fact]
    public async Task ToCreatedResultAsync_ShouldReturn404_OnNotFoundResult()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpContext.Returns(httpContext);

        var notFoundResult = Result<TestDto>.NotFound("Not found");
        var routeValues = new { id = 0 };

        // Act
        await endpoint.ToCreatedResultAsync(notFoundResult, routeValues, CancellationToken.None);

        // Assert
        httpContext.Response.StatusCode.ShouldBe(404);
    }

    private sealed record TestDto(long Id, string Name);
}
