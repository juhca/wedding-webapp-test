using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// Composite email provider that tries each registered provider in priority order.
/// Per provider: up to <see cref="MaxRetriesPerProvider"/> attempts with exponential backoff.
/// On permanent failure (4xx/5xx codes) retries are skipped and the next provider is tried immediately.
/// Throws <see cref="InvalidOperationException"/> only if every provider exhausts all retries.
/// </summary>
public class EmailProviderFallback : IEmailProvider
{
    private readonly IEmailProvider[] _providers;
    private readonly ILogger<EmailProviderFallback> _logger;
    private const int MaxRetriesPerProvider = 3;

    public EmailProviderFallback(
        [FromKeyedServices("resend")] IEmailProvider resend,
        [FromKeyedServices("smtp")] IEmailProvider smtp,
        ILogger<EmailProviderFallback> logger)
    {
        _providers = [resend, smtp];
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        foreach (var provider in _providers)
        {
            var name = provider.GetType().Name;
            var shouldSwitchProvider = false;

            for (int attempt = 1; attempt <= MaxRetriesPerProvider; attempt++)
            {
                try
                {
                    await provider.SendAsync(to, subject, htmlBody, ct);

                    _logger.LogInformation(
                        "Email sent to {To} via {Provider} on attempt {Attempt}",
                        to, name, attempt);

                    return;
                }
                catch (EmailProviderException ex) when (ex.IsPermanent)
                {
                    _logger.LogError(ex,
                        "{Provider} reported a permanent failure sending to {To} — skipping provider",
                        name, to);

                    shouldSwitchProvider = true;
                    break;
                }
                catch (EmailProviderException ex)
                {
                    _logger.LogWarning(ex,
                        "{Provider} transient failure sending to {To} (attempt {Attempt}/{Max})",
                        name, to, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct); // 2s then 4s
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex,
                        "{Provider} unexpected failure sending to {To} (attempt {Attempt}/{Max})",
                        name, to, attempt, MaxRetriesPerProvider);

                    if (attempt < MaxRetriesPerProvider)
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                }
            }

            if (!shouldSwitchProvider)
            {
                _logger.LogError(
                    "All {Max} retries exhausted for {Provider} sending to {To} — switching to next provider",
                    MaxRetriesPerProvider, name, to);
            }
        }

        throw new InvalidOperationException(
            $"All email providers failed to send to {to}. Email was not delivered.");
    }
}
