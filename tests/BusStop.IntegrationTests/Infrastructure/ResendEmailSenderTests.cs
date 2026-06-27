using BusStop.Core.NotificationAggregate.Interfaces;
using BusStop.Infrastructure.Integrations.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Resend;
using Shouldly;
using Xunit;

namespace BusStop.IntegrationTests.Infrastructure;

[Collection("Sequential")]
public class ResendEmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_InDevelopmentEnvironment_ShouldRedirectToResendSandbox()
    {
        // Arrange
        var mockResend = Substitute.For<IResend>();
        mockResend.EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResendResponse<Guid> { Content = Guid.NewGuid(), Success = true }));

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Development");

        var mockLogger = Substitute.For<ILogger<ResendEmailSender>>();

        var sender = new ResendEmailSender(mockResend, mockEnv, mockLogger);

        // Act
        await sender.SendEmailAsync("real.user@example.com", "Test Subject", "Test Body");

        // Assert
        await mockResend.Received(1).EmailSendAsync(
            Arg.Is<EmailMessage>(m => 
                m.To.Count == 1 && 
                m.To.Contains("delivered@resend.dev") && 
                !m.To.Contains("real.user@example.com") &&
                m.Subject == "Test Subject" &&
                m.HtmlBody == "Test Body"
            ),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailAsync_InProductionEnvironment_ShouldSendToOriginalAddress()
    {
        // Arrange
        var mockResend = Substitute.For<IResend>();
        mockResend.EmailSendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResendResponse<Guid> { Content = Guid.NewGuid(), Success = true }));

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Production");

        var mockLogger = Substitute.For<ILogger<ResendEmailSender>>();

        var sender = new ResendEmailSender(mockResend, mockEnv, mockLogger);

        // Act
        await sender.SendEmailAsync("real.user@example.com", "Test Subject", "Test Body");

        // Assert
        await mockResend.Received(1).EmailSendAsync(
            Arg.Is<EmailMessage>(m => 
                m.To.Count == 1 && 
                m.To.Contains("real.user@example.com") &&
                m.Subject == "Test Subject"
            ),
            Arg.Any<CancellationToken>());
    }
}
