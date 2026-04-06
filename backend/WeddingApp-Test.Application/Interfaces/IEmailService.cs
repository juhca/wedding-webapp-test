namespace WeddingApp_Test.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string recipientEmail, string subject, string body, CancellationToken ct = default);
}
