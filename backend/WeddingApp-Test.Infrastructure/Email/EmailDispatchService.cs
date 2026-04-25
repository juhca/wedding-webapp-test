using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Infrastructure.Email;

public class EmailDispatchService(IEmailTemplateRepository templateRepo, IEmailOutboxRepository outboxRepo,
    IEmailSendLogRepository sendLogRepo,
    IEmailEventChannel channel,
    ILiquidRenderer renderer,
    IWeddingInfoRepository weddingInfoRepo,
    ILogger<EmailDispatchService> logger) : IEmailDispatchService
{
    public async Task DispatchEventAsync(string eventName, User triggeredBy, Dictionary<string, object?> context, CancellationToken ct)
    {
        // Skip if user has no email address — nothing to send to
        if (string.IsNullOrWhiteSpace(triggeredBy.Email))
        {
            logger.LogDebug("Skipping dispatch for event {Event}: user {UserId} has no email", eventName, triggeredBy.Id);
            return;
        }
        
        // Load active templates matching this event
        var templates = await templateRepo.GetActiveByTriggerAsync(eventName, triggeredBy.Id);
        if (!templates.Any())
        {
            logger.LogDebug("No active templates for event {Event}", eventName);
            return;
        }
        
        // Build the render model: merge caller-supplied context + user + wedding info
        var weddingInfo = await weddingInfoRepo.GetWeddingInfoAsync();
        var renderModel = new Dictionary<string, object?>(context)
        {
            ["User"]    = triggeredBy,
            ["Wedding"] = weddingInfo
        };
        
        foreach (var template in templates)
        {
            // Dedup check: skip if user already received max allowed sends for this template
            if (template.MaxSendsPerUser.HasValue)
            {
                var alreadySent = await sendLogRepo.CountDispatchedAsync(template.Id, triggeredBy.Id);
                if (alreadySent >= template.MaxSendsPerUser.Value)
                {
                    logger.LogDebug("Skipping template {TemplateId} for user {UserId}: dedup limit reached", template.Id, triggeredBy.Id);
                    continue;
                }
            }

            // Render the three Liquid templates
            string subject, htmlBody;
            string? plainTextBody = null;
            try
            {
                subject = await renderer.RenderAsync(template.SubjectTemplate, renderModel);
                htmlBody = await renderer.RenderAsync(template.HtmlBodyTemplate, renderModel);
                if (template.PlainTextBodyTemplate is not null)
                {
                    plainTextBody = await renderer.RenderAsync(template.PlainTextBodyTemplate, renderModel);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Liquid render failed for template {TemplateId}", template.Id);
                continue;
            }

            // Grab optional related entity ID from context (e.g. the RSVP's Guid)
            Guid? relatedId = context.TryGetValue("RelatedEntityId", out var rid) && rid is Guid g ? g : null;

            // Write the outbox record
            var outbox = new EmailOutbox
            {
                Id = Guid.NewGuid(),
                ToEmail = triggeredBy.Email,
                ToName = $"{triggeredBy.FirstName} {triggeredBy.LastName}".Trim(),
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody,
                Status = EmailStatus.Pending,
                EmailType = eventName,
                RelatedEntityId = relatedId,
                CreatedAt = DateTime.UtcNow
            };

            await outboxRepo.AddAsync(outbox);
            await outboxRepo.SaveChangesAsync();

            // Write the dispatch log entry (for dedup tracking).
            // This log is only used by CountDispatchedAsync to enforce MaxSendsPerUser.
            // IMPORTANT: Dispatched=true means "outbox record was created without error".
            // It does NOT mean the email was delivered. Delivery result is in EmailOutbox.Status.
            await sendLogRepo.AddAsync(new EmailSendLog
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                UserId = triggeredBy.Id,
                Dispatched = true,
                DispatchedAt = DateTime.UtcNow
            });
            await sendLogRepo.SaveChangesAsync();

            // Notify the background processor that a new outbox record is waiting
            await channel.PublishAsync(outbox.Id, ct);

            logger.LogInformation("Queued email type {Event} for {Email} (outbox {OutboxId})", eventName, triggeredBy.Email, outbox.Id);
        }
    }
}
