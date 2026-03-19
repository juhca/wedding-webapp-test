namespace WeddingApp_Test.Application.Interfaces;

/// <summary>
/// Low-level email sending abstraction. Throws <see cref="EmailProviderException"/> on failure.
/// Implementations: EmailProviderResend, EmailProviderSmtp, EmailProviderFallback.
/// </summary>
public interface IEmailProvider
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
