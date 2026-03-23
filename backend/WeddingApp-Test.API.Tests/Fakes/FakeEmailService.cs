using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.API.Tests.Fakes;

/// <summary>
/// No-op email service used in integration tests to prevent real emails from being sent.
/// </summary>
public class FakeEmailService : IEmailService
{
    public Task SendReminderEmailAsync(string recipientEmail, Reminder reminder, CancellationToken ct = default)
        => Task.CompletedTask;
}
