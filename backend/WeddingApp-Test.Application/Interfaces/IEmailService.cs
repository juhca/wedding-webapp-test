using WeddingApp_Test.Domain.Entities;
using System.Threading;

namespace WeddingApp_Test.Application.Interfaces;

public interface IEmailService
{
    Task SendReminderEmailAsync(string recipientEmail, Reminder reminder, CancellationToken ct = default);
}
