using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Services;

public class EmailService(
    IEnumerable<IEmailProvider> providers,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task SendReminderEmailAsync(string recipientEmail, Reminder reminder)
    {
        var message = new ReminderEmailMessage(reminder);
        var providerList = providers.ToList();

        foreach (var provider in providerList)
        {
            try
            {
                await provider.SendAsync(recipientEmail, message);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Email provider '{Provider}' failed for reminder {ReminderId}. Trying next.",
                    provider.Name, reminder.Id);
            }
        }

        logger.LogCritical(
            "All {Count} email provider(s) failed for reminder {ReminderId}. Check your email configuration.",
            providerList.Count, reminder.Id);
    }
}
