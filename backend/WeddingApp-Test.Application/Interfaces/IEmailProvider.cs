using WeddingApp_Test.Application.Email;

namespace WeddingApp_Test.Application.Interfaces;

public interface IEmailProvider
{
    string Name { get; }
    Task SendAsync(string recipientEmail, EmailMessage message, CancellationToken ct = default);
}
