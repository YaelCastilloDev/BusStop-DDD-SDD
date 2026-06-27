using BusStop.Core.NotificationAggregate.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resend;

namespace BusStop.Infrastructure.Integrations.Email;

public class ResendEmailSender(IResend resend, IHostEnvironment env, ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // For local development, redirect all emails to Resend's safe sandbox testing address
        // to verify API connectivity without sending actual emails.
        if (env.IsDevelopment())
        {
            logger.LogInformation("Development environment detected. Redirecting email from {OriginalTo} to delivered@resend.dev", to);
            to = "delivered@resend.dev";
        }

        var message = new EmailMessage
        {
            From = "noreply@busstop.local", // In a real app, this should come from configuration and be a verified domain
            To = { to },
            Subject = subject,
            HtmlBody = body
        };

        logger.LogInformation("Sending email via Resend to {To}. Subject: {Subject}", to, subject);
        
        var response = await resend.EmailSendAsync(message, cancellationToken);
        logger.LogInformation("Successfully sent email to {To}. Resend Email ID: {EmailId}", to, response.Content);
    }
}
