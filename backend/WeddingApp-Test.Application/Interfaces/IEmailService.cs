using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IEmailService
{
    Task SendReminderEmailAsync(Reminder reminder);
}
