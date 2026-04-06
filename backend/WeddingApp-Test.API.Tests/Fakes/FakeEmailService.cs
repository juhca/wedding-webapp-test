using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.Tests.Fakes;

/// <summary>
/// No-op email service used in integration tests to prevent real emails from being sent.
/// </summary>
public class FakeEmailService : IEmailService
{
    public Task SendAsync(string recipientEmail, string subject, string body, CancellationToken ct = default)
        => Task.CompletedTask;
}
