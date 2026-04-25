using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Infrastructure.Email;

public class EmailOutboxProcessorService(IEmailOutboxRepository outboxRepo, IEmailSender emailSender, IOptions<EmailOptions> options, ILogger<EmailOutboxProcessorService> logger)
{
    public async Task ProcessByIdAsync(Guid outboxId, CancellationToken ct)
    {
        var outbox = await outboxRepo.GetByIdAsync(outboxId);
        if (outbox is null)
        {
            logger.LogWarning("Outbox record {Id} not found", outboxId);
            return;
        }
        
        // Skip if already proccessed
        if (outbox.Status is EmailStatus.Sent or EmailStatus.Failed)
        {
            logger.LogDebug("Outbox {Id} already in terminal state {Status} — skipping", outboxId, outbox.Status);
            return;
        }
        
        var opt = options.Value;
        var success = await emailSender.SendAsync(outbox.ToEmail, outbox.ToName, outbox.Subject, outbox.HtmlBody, outbox.PlainTextBody, ct);
        outbox.AttemptCount++;

        if (success)
        {
            outbox.Status = EmailStatus.Sent;
            outbox.SentAt = DateTime.UtcNow;
            logger.LogInformation("Outbox {Id} sent successfully", outboxId);
        }
        else
        {
            outbox.LastError = "All providers failed";

            if (outbox.AttemptCount >= opt.MaxAttempts)
            {
                outbox.Status = EmailStatus.Failed;
                logger.LogError("Outbox {Id} failed after {Attempts} attempts — marking Failed", outboxId, outbox.AttemptCount);
            }
            else
            {
                // Exponential backoff: use the delay for this attempt tier
                // RetryDelayMinutes = [1, 5, 30, 360, 1440] → attempt 0=1m, 1=5m, 2=30m, 3=6h, 4=24h
                var delays = opt.RetryDelayMinutes;
                var tierIndex = Math.Min(outbox.AttemptCount - 1, delays.Length - 1);
                var delayMinutes = delays[tierIndex];
                outbox.NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
                logger.LogWarning("Outbox {Id} failed (attempt {Attempt}/{Max}), retry in {Minutes}m at {RetryAt}",
                    outboxId, outbox.AttemptCount, opt.MaxAttempts, delayMinutes, outbox.NextRetryAt);
            }
        }

        outboxRepo.Update(outbox);
        await outboxRepo.SaveChangesAsync();
    }

    public async Task ProcessPendingRetriesAsync(CancellationToken ct)
    {
        var pending = await outboxRepo.GetPendingRetryableAsync(DateTime.UtcNow);
        foreach (var record in pending)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }
            await ProcessByIdAsync(record.Id, ct);
        }
    }
}