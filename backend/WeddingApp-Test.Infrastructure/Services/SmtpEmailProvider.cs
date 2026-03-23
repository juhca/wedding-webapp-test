using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Services;

public class SmtpEmailProvider(IOptions<SmtpOptions> options, ILogger<SmtpEmailProvider> logger) : IEmailProvider
{
    private readonly SmtpOptions _options = options.Value;

    public string Name => "SMTP";

    public async Task SendAsync(string recipientEmail, EmailMessage message, CancellationToken ct = default)
    {
        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(_options.FromDisplayName, _options.FromAddress));
        mail.To.Add(MailboxAddress.Parse(recipientEmail));
        mail.Subject = message.Subject;
        mail.Body = new TextPart("plain") { Text = message.Body };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            await client.SendAsync(mail, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new EmailProviderException($"SMTP failed: {ex.Message}", isPermanent: false, inner: ex);
        }

        logger.LogInformation("{Provider}: email sent to {Recipient}.", Name, recipientEmail);
    }
}
