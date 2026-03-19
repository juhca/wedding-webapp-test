using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// Sends emails via SMTP using MailKit. Works with Gmail (App Passwords), Outlook, or any SMTP server.
/// Bind config from appsettings: EmailConfig:Smtp
/// </summary>
public class EmailProviderSmtp : IEmailProvider
{
    private readonly SmtpConfig _config;

    public EmailProviderSmtp(IOptions<SmtpConfig> config)
    {
        _config = config.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.FromName, _config.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Headers.Add("X-Message-Id", Guid.NewGuid().ToString());
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_config.Username, _config.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (SmtpCommandException ex)
        {
            // 5xx SMTP command failures — invalid address, mailbox full, etc.
            // StatusCode >= 500 is usually a permanent rejection
            var isPermanent = (int)ex.StatusCode >= 500;
            throw new EmailProviderException(
                $"SMTP command error ({ex.StatusCode}): {ex.Message}",
                isPermanent,
                ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Network issues, auth failures, protocol errors — transient, try another provider
            throw new EmailProviderException(
                $"SMTP transient failure: {ex.Message}",
                isPermanent: false,
                ex);
        }
    }
}
