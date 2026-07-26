using BusStop.Core.Interfaces;
using BusStop.Web.Configurations;
using Mediator;
using Polly;
using Polly.Registry;
using Shouldly;
using Xunit;

namespace BusStop.UnitTests.Web.Configurations;

public sealed class ResilienceBehaviorTests
{
    // Dummy request types for testing
    private sealed record IdempotentTestRequest : IRequest<string>, IIdempotentRequest;
    private sealed record NonIdempotentTestRequest : IRequest<string>;

    private static ResiliencePipelineProvider<string> CreateProviderWithRecordedKey(
        out Func<string?> getRequestedKey)
    {
        string? requestedKey = null;
        getRequestedKey = () => requestedKey;

        var substitute = Substitute.For<ResiliencePipelineProvider<string>>();
        substitute
            .GetPipeline(Arg.Any<string>())
            .Returns(ci =>
            {
                requestedKey = ci.Arg<string>();
                return ResiliencePipeline.Empty;
            });

        return substitute;
    }

    [Fact]
    public async Task Handle_IdempotentRequest_UsesIdempotentPipelineKey()
    {
        // Arrange
        var provider = CreateProviderWithRecordedKey(out var getRequestedKey);
        var behavior = new ResilienceBehavior<IdempotentTestRequest, string>(provider);

        // Act
        await behavior.Handle(
            new IdempotentTestRequest(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        getRequestedKey().ShouldBe("idempotent-mediator",
            "Idempotent requests must use the idempotent-mediator pipeline key");
        provider.Received(1).GetPipeline("idempotent-mediator");
    }

    [Fact]
    public async Task Handle_NonIdempotentRequest_UsesNonIdempotentPipelineKey()
    {
        // Arrange
        var provider = CreateProviderWithRecordedKey(out var getRequestedKey);
        var behavior = new ResilienceBehavior<NonIdempotentTestRequest, string>(provider);

        // Act
        await behavior.Handle(
            new NonIdempotentTestRequest(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        getRequestedKey().ShouldBe("non-idempotent-mediator",
            "Non-idempotent requests must use the non-idempotent-mediator pipeline key");
        provider.Received(1).GetPipeline("non-idempotent-mediator");
    }

    [Fact]
    public async Task Handle_DelegatesToNextHandler_AndReturnsResponse()
    {
        // Arrange
        var provider = CreateProviderWithRecordedKey(out _);
        var behavior = new ResilienceBehavior<IdempotentTestRequest, string>(provider);
        var expectedResponse = "expected-result";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(
            new IdempotentTestRequest(),
            (req, ct) =>
            {
                nextCalled = true;
                req.ShouldBeOfType<IdempotentTestRequest>();
                return ValueTask.FromResult(expectedResponse);
            },
            CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
        nextCalled.ShouldBeTrue("the next handler must be invoked");
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToNextHandler()
    {
        // Arrange
        var provider = CreateProviderWithRecordedKey(out _);
        var behavior = new ResilienceBehavior<IdempotentTestRequest, string>(provider);
        using var cts = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;

        // Act
        await behavior.Handle(
            new IdempotentTestRequest(),
            (_, ct) =>
            {
                capturedToken = ct;
                return ValueTask.FromResult("ok");
            },
            cts.Token);

        // Assert - the token passed to the pipeline context should be the same
        capturedToken.ShouldBe(cts.Token,
            "the CancellationToken must be propagated through the pipeline");
    }
}
