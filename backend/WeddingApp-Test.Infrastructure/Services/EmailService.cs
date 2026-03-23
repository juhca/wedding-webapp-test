using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Services;

public class EmailService(IEnumerable<IEmailProvider> providers, ILogger<EmailService> logger) : IEmailService
{
    private const int MaxRetriesPerProvider = 3;

    public async Task SendReminderEmailAsync(string recipientEmail, Reminder reminder, CancellationToken ct = default)
    {
        var message = new ReminderEmailMessage(reminder);
        var providerList = providers.ToList();
        var hadTransientFailure = false;

        foreach (var provider in providerList)
        {
            var shouldSwitchProvider = false;

            for (int attempt = 1; attempt <= MaxRetriesPerProvider; attempt++)
            {
                try
                {
                    await provider.SendAsync(recipientEmail, message, ct);

                    logger.LogInformation("Reminder {ReminderId} sent to {Recipient} via {Provider} on attempt {Attempt}.", reminder.Id, recipientEmail, provider.Name, attempt);

                    return;
                }
                catch (EmailProviderException ex) when (ex.IsPermanent)
                {
                    logger.LogError(ex, "{Provider} reported a permanent failure for reminder {ReminderId} — skipping provider.", provider.Name, reminder.Id);

                    shouldSwitchProvider = true;
                    break;
                }
                catch (EmailProviderException ex)
                {
                    logger.LogWarning(ex, "{Provider} transient failure for reminder {ReminderId} (attempt {Attempt}/{Max}).", provider.Name, reminder.Id, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct); // 2s then 4s
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "{Provider} unexpected failure for reminder {ReminderId} (attempt {Attempt}/{Max}).", provider.Name, reminder.Id, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                    }
                }
            }

            if (!shouldSwitchProvider)
            {
                // retries exhausted transiently — not a permanent/recipient-specific issue
                hadTransientFailure = true;
                logger.LogError("All {Max} retries exhausted for {Provider} on reminder {ReminderId} — switching to next provider.", MaxRetriesPerProvider, provider.Name, reminder.Id);
            }
        }

        logger.LogCritical("All {Count} email provider(s) failed for reminder {ReminderId}. Email was not delivered.", providerList.Count, reminder.Id);
        throw new EmailDeliveryException(reminder.Id, isTransient: hadTransientFailure);
    }
}
