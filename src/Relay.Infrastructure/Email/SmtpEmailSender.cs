#pragma warning disable SYSLIB0014
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Relay.Application.Services;

namespace Relay.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendPasswordResetAsync(string recipient, string displayName, string resetLink, CancellationToken cancellationToken)
    {
        if (!bool.TryParse(configuration["Mail:Enabled"], out var enabled) || !enabled) return;
        var host = configuration["Mail:Host"];
        if (string.IsNullOrWhiteSpace(host)) return;
        var port = int.TryParse(configuration["Mail:Port"], out var configuredPort) ? configuredPort : 2525;
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(configuration["Mail:Username"], configuration["Mail:Password"])
        };
        using var message = new MailMessage(
            new MailAddress(configuration["Mail:FromEmail"] ?? "no-reply@example.com", configuration["Mail:FromName"] ?? "Relay"),
            new MailAddress(recipient, displayName))
        {
            Subject = "Reset your Relay password",
            Body = $"Use this one-time link within the configured expiry window: {resetLink}",
            IsBodyHtml = false
        };
        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(string recipient, string displayName, string code, CancellationToken cancellationToken)
    {
        if (!bool.TryParse(configuration["Mail:Enabled"], out var enabled) || !enabled) return;
        var host = configuration["Mail:Host"];
        if (string.IsNullOrWhiteSpace(host)) return;
        var port = int.TryParse(configuration["Mail:Port"], out var configuredPort) ? configuredPort : 2525;
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(configuration["Mail:Username"], configuration["Mail:Password"])
        };
        using var message = new MailMessage(
            new MailAddress(configuration["Mail:FromEmail"] ?? "no-reply@example.com", configuration["Mail:FromName"] ?? "Relay"),
            new MailAddress(recipient, displayName))
        {
            Subject = "Your Relay verification code",
            Body = $"Your verification code is {code}. It expires in 15 minutes.",
            IsBodyHtml = false
        };
        await client.SendMailAsync(message, cancellationToken);
    }

}
#pragma warning restore SYSLIB0014
