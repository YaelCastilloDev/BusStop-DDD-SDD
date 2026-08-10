using BusStop.Core.Interfaces;
using BusStop.Infrastructure.Integrations.Email;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resend;

namespace BusStop.UnitTests.Integrations.Email;

public class ResendEmailSenderTests
{
    /// <summary>
    /// Documents a bug in <see cref="ResendEmailSender.SendEmailAsync"/>:
    /// the method does not check whether <see cref="IResend.EmailSendAsync"/> succeeded
    /// and has no try-catch around the Resend call. Any exception from the Resend client
    /// (network error, auth failure, API error) propagates up unhandled.
    /// The method also unconditionally logs "Successfully sent" without inspecting the response.
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_ShouldCheckResponseSuccess_PropagatesExceptionWhenResendFails()
    {
        // Arrange
        var mockResend = Substitute.For<IResend>();
        mockResend.EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<ResendResponse<Guid>>>(_ =>
                throw new HttpRequestException("Simulated Resend API failure (network error, auth failure, etc.)"));

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Production");

        var mockLogger = Substitute.For<ILogger<ResendEmailSender>>();

        var sender = new ResendEmailSender(mockResend, mockEnv, mockLogger);

        // Act & Assert: The exception propagates unhandled because SendEmailAsync
        // has no try-catch block around the Resend call. This documents the gap:
        // the method should catch exceptions and log/handle them gracefully,
        // or at minimum check the ResendResponse for success before logging
        // "Successfully sent".
        var ex = await Should.ThrowAsync<HttpRequestException>(() =>
            sender.SendEmailAsync("test@example.com", "Test Subject", "Test Body", TestContext.Current.CancellationToken));

        ex.Message.ShouldBe("Simulated Resend API failure (network error, auth failure, etc.)");
    }

    /// <summary>
    /// Documents the expected behavior: the response should be checked before
    /// assuming success. Currently the method logs "Successfully sent" regardless
    /// of whether the Resend API actually accepted the email.
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_LogsSuccess_WithoutCheckingResponse()
    {
        // Arrange
        var mockResend = Substitute.For<IResend>();
        // ResendResponse has no IsSuccess property — the method never validates the response.
        mockResend.EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit())));

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Production");

        var mockLogger = Substitute.For<ILogger<ResendEmailSender>>();

        var sender = new ResendEmailSender(mockResend, mockEnv, mockLogger);

        // Act
        await sender.SendEmailAsync("test@example.com", "Test Subject", "Test Body", TestContext.Current.CancellationToken);

        // Assert: The call succeeded (no exception) because IResend didn't throw.
        // However, note that SendEmailAsync never inspects whether the ResendResponse
        // indicates success — it always logs "Successfully sent email".
        // This test passes, confirming the response is not validated.
        await mockResend.Received(1).EmailSendAsync(
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailAsync_UsesHardcodedFromAddress()
    {
        EmailMessage? capturedMessage = null;

        var mockResend = Substitute.For<IResend>();
        mockResend.EmailSendAsync(
            Arg.Do<EmailMessage>(msg => capturedMessage = msg),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit())));

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Production");

        var mockLogger = Substitute.For<ILogger<ResendEmailSender>>();

        var sender = new ResendEmailSender(mockResend, mockEnv, mockLogger);

        await sender.SendEmailAsync("test@example.com", "Test Subject", "Test Body", TestContext.Current.CancellationToken);

        capturedMessage.ShouldNotBeNull();
        capturedMessage!.From.ShouldBe("noreply@busstop.local");
    }
}
