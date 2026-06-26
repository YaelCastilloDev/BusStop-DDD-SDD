using BusStop.Core.NotificationAggregate.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusStop.Infrastructure.Integrations.Email;

public class DummyEmailSender(ILogger<DummyEmailSender> logger) : IEmailSender
{
  public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
  {
    logger.LogInformation("Sending email to {To}. Subject: {Subject}. Body: {Body}", to, subject, body);
    return Task.CompletedTask;
  }
}
