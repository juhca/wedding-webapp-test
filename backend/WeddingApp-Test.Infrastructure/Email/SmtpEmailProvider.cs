using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces.Email;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace WeddingApp_Test.Infrastructure.Email;

public class SmtpEmailProvider(IOptions<EmailOptions> options, ILogger<SmtpEmailProvider> logger) : IEmailProvider
{
    public string Name => "SMTP";

    public async Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string htmlBody, string? plainTextBody, CancellationToken ct)
    {
        var smtp = options.Value.Smtp;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toEmail, toName));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = plainTextBody
            };
            message.Body = builder.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(smtp.Host, smtp.Port,
                smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);
            await smtpClient.AuthenticateAsync(smtp.Username, smtp.Password, ct);
            await smtpClient.SendAsync(message, ct);
            await smtpClient.DisconnectAsync(true, ct);
        }
        catch (Exception ex) when (ex is not EmailDeliveryException)
        {
            logger.LogWarning(ex, "SMTP send failed");
            throw new EmailDeliveryException($"SMTP send failed: {ex.Message}", ex);
        }
    }
}