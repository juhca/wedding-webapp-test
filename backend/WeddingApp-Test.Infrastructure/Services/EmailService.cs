using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Services;

public class EmailService(IEnumerable<IEmailProvider> providers, ILogger<EmailService> logger) : IEmailService
{
    private const int MaxRetriesPerProvider = 3;

    public async Task SendAsync(string recipientEmail, string subject, string body, CancellationToken ct = default)
    {
        var message = new PlainEmail(subject, body);
        await SendWithRetryAsync(recipientEmail, message, ct);
    }

    private async Task SendWithRetryAsync(string recipientEmail, EmailMessage message, CancellationToken ct)
    {
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

                    logger.LogInformation("Email sent to {Recipient} via {Provider} on attempt {Attempt}.", recipientEmail, provider.Name, attempt);

                    return;
                }
                catch (EmailProviderException ex) when (ex.IsPermanent)
                {
                    logger.LogError(ex, "{Provider} reported a permanent failure — skipping provider.", provider.Name);

                    shouldSwitchProvider = true;
                    break;
                }
                catch (EmailProviderException ex)
                {
                    logger.LogWarning(ex, "{Provider} transient failure (attempt {Attempt}/{Max}).", provider.Name, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "{Provider} unexpected failure (attempt {Attempt}/{Max}).", provider.Name, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                }
            }

            if (!shouldSwitchProvider)
            {
                hadTransientFailure = true;
                logger.LogError("All {Max} retries exhausted for {Provider} — switching to next provider.", MaxRetriesPerProvider, provider.Name);
            }
        }

        logger.LogCritical("All {Count} email provider(s) failed. Email to {Recipient} was not delivered.", providerList.Count, recipientEmail);
        throw new EmailDeliveryException(Guid.Empty, isTransient: hadTransientFailure);
    }
}
