using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Services;

public class SmtpEmailProvider(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailProvider> logger) : IEmailProvider
{
    private readonly SmtpOptions _options = options.Value;

    public string Name => "SMTP";

    public async Task SendAsync(string recipientEmail, EmailMessage message)
    {
        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(_options.FromDisplayName, _options.FromAddress));
        mail.To.Add(MailboxAddress.Parse(recipientEmail));
        mail.Subject = message.Subject;
        mail.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        await client.SendAsync(mail);
        await client.DisconnectAsync(true);

        logger.LogInformation("{Provider}: email sent to {Recipient}.", Name, recipientEmail);
    }
}
