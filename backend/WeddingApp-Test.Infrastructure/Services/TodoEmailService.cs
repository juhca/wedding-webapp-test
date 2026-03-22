using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Services;

public class TodoEmailService(ILogger<TodoEmailService> logger) : IEmailService
{
    public Task SendReminderEmailAsync(Reminder reminder)
    {
        logger.LogInformation("[EMAIL STUB] Sending reminder {ReminderId} (Type={Type}, TargetId={TargetId}, ScheduledFor={ScheduledFor:yyyy-MM-dd})",
            reminder.Id, reminder.Type, reminder.TargetId, reminder.ScheduledFor);

        return Task.CompletedTask;
    }
}
