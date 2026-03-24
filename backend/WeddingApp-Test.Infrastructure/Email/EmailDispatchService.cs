using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Email;

public class EmailDispatchService(AppDbContext db, IEmailService emailService, ILiquidRenderer liquidRenderer, ILogger<EmailDispatchService> logger) : IEmailDispatchService
{
    public async Task DispatchEventAsync(string eventName, User triggeredBy, Dictionary<string, object?> context, CancellationToken ct = default)
    {
        var templates = await db.EmailTemplates
            .Where(t => t.TriggerName == eventName && t.IsActive)
            .Include(t => t.SendLogs.Where(l => l.UserId == triggeredBy.Id))
            .ToListAsync(ct);

        if (templates.Count == 0)
        {
            logger.LogDebug("No active templates found for event {Event}.", eventName);
            
            return;
        }

        // Fetch wedding info once for all templates
        var wedding = await db.WeddingInfo.FirstOrDefaultAsync(ct);

        var renderModel = new Dictionary<string, object?>(context)
        {
            ["user"] = triggeredBy,
            ["wedding"] = wedding
        };

        foreach (var template in templates)
        {
            // Deduplication: skip if MaxSendsPerUser already reached for this user
            if (template.MaxSendsPerUser.HasValue)
            {
                var sendCount = template.SendLogs.Count(l => l.UserId == triggeredBy.Id && l.Succeeded);
                if (sendCount >= template.MaxSendsPerUser.Value)
                {
                    logger.LogDebug("Template {TemplateId} already sent {Count} time(s) to user {UserId} — skipping.",
                        template.Id, sendCount, triggeredBy.Id);
                    continue;
                }
            }

            var log = new EmailSendLog
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                UserId = triggeredBy.Id,
                SentAt = DateTime.UtcNow
            };

            try
            {
                var subject = await liquidRenderer.RenderAsync(template.Subject, renderModel);
                var body = await liquidRenderer.RenderAsync(template.Body, renderModel);

                await emailService.SendAsync(triggeredBy.Email, subject, body, ct);

                log.Succeeded = true;
                logger.LogInformation("Dispatched template {TemplateId} ({TemplateName}) to {Email} for event {Event}.",
                    template.Id, template.Name, triggeredBy.Email, eventName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.Succeeded = false;
                log.Error = ex.Message;
                logger.LogError(ex, "Failed to dispatch template {TemplateId} to {Email} for event {Event}.",
                    template.Id, triggeredBy.Email, eventName);
            }
            finally
            {
                db.EmailSendLogs.Add(log);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
