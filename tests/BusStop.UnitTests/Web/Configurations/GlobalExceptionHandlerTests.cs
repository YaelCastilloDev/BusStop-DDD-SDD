using BusStop.Web.Configurations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BusStop.UnitTests.Web.Configurations;

/// <summary>
/// Regression tests for <see cref="GlobalExceptionHandler"/>.
/// </summary>
public sealed class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandlerTests()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    }

    [Fact]
    public async Task ShouldReturn499_ForOperationCanceledException()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(_logger);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var canceledException = new OperationCanceledException("Client disconnected");

        // Act
        var handled = await handler.TryHandleAsync(
            httpContext,
            canceledException,
            CancellationToken.None);

        // Assert
        handled.ShouldBeTrue("OperationCanceledException should be handled");
        httpContext.Response.StatusCode.ShouldBe(499);

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(s => s.ToString()!.Contains("Request canceled")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Verifies that a generic exception still returns 500 (current behavior).
    /// This test should PASS — it documents the expected behavior for real server errors.
    /// </summary>
    [Fact]
    public async Task ShouldReturn500_ForGenericException()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(_logger);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var genericException = new InvalidOperationException("Something unexpected happened");

        // Act
        var handled = await handler.TryHandleAsync(
            httpContext,
            genericException,
            CancellationToken.None);

        // Assert
        handled.ShouldBeTrue("all exceptions should be handled");
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        // WriteAsJsonAsync sets ContentType to "application/json; charset=utf-8"
        // The handler sets it to "application/problem+json" but WriteAsJsonAsync may override it.
        // Either value is acceptable for the generic error case.
        httpContext.Response.ContentType.ShouldNotBeNull();
        (httpContext.Response.ContentType!.Contains("application/json")).ShouldBeTrue(
            "response should have a JSON content type");
    }

    /// <summary>
    /// Verifies the handler logs the exception at Error level.
    /// </summary>
    [Fact]
    public async Task ShouldLogException_AsError()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(_logger);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var exception = new Exception("test exception");

        // Act
        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(s => s.ToString()!.Contains("test exception")),
            Arg.Is<Exception>(e => e.Message == "test exception"),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
