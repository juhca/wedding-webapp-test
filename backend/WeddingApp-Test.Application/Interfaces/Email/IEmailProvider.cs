namespace WeddingApp_Test.Application.Interfaces.Email;

//One per email service (Resend, SMTP). Throws on failure. TODO: should catch the failures retry if possible, else try other provider
public interface IEmailProvider
{
    string Name { get; }

    Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string htmlBody, string? plainTextBody, CancellationToken ct);
}