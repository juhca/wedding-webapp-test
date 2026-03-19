using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Infrastructure.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.BackgroundServices;

/// <summary>
/// Processes ScheduledRelative and ScheduledAbsolute email templates.
/// Runs every hour, picks up any EmailSchedule rows due today that haven't been sent.
///
/// Per-user schedules (UserId != null): send to that user only.
/// Bulk schedules (UserId == null): resolve audience via AudienceType, dedup via EmailSendLog.
/// </summary>
public class EmailSchedulerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSchedulerService> _logger;

    public EmailSchedulerService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<EmailSchedulerService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Short startup delay, then check every hour
        await Task.Delay(TimeSpan.FromMinutes(1), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedulesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "EmailSchedulerService unhandled error");
            }

            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }

    // ─── Main loop ──────────────────────────────────────────────────

    private async Task ProcessDueSchedulesAsync(CancellationToken ct)
    {
        await using var scope = _services.CreateAsyncScope();
        var db       = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var renderer = scope.ServiceProvider.GetRequiredService<ILiquidRenderer>();
        var provider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();

        var dueSchedules = await db.EmailSchedules
            .Include(s => s.Template)
            .Where(s => s.ScheduledFor.Date <= DateTime.UtcNow.Date
                     && s.SentAt == null
                     && s.Template.IsActive)
            .ToListAsync(ct);

        _logger.LogInformation("EmailSchedulerService: {Count} schedules due", dueSchedules.Count);

        foreach (var schedule in dueSchedules)
        {
            try
            {
                if (schedule.UserId is not null)
                    await ProcessPerUserAsync(schedule, db, renderer, provider, ct);
                else
                    await ProcessBulkAsync(schedule, db, renderer, provider, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to process schedule {Id} (template: {Name})",
                    schedule.Id, schedule.Template.Name);
                // Do NOT mark SentAt — will retry on next run
            }
        }
    }

    // ─── Per-user schedule ───────────────────────────────────────────

    private async Task ProcessPerUserAsync(
        EmailSchedule schedule, AppDbContext db,
        ILiquidRenderer renderer, IEmailProvider provider, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([schedule.UserId!.Value], ct);
        if (user is null)
        {
            _logger.LogWarning("Per-user schedule {Id}: user {UserId} not found — skipping", schedule.Id, schedule.UserId);
            schedule.SentAt = DateTime.UtcNow; // Mark done so we don't retry
            await db.SaveChangesAsync(ct);
            return;
        }

        // Dedup
        if (schedule.Template.MaxSendsPerUser.HasValue)
        {
            var sentCount = await db.EmailSendLogs
                .CountAsync(l => l.TemplateId == schedule.TemplateId
                              && l.UserId == user.Id
                              && l.Status == "Sent", ct);

            if (sentCount >= schedule.Template.MaxSendsPerUser.Value)
            {
                _logger.LogDebug("Template {Name} already sent {N} times to {UserId} — skipping",
                    schedule.Template.Name, sentCount, user.Id);
                schedule.SentAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                return;
            }
        }

        var weddingInfo = await db.WeddingInfo.FirstOrDefaultAsync(ct);
        var context = WeddingApp_Test.Infrastructure.Email.EmailDispatchService.BuildBaseContext(user, weddingInfo, _config["EmailConfig:ApiBaseUrl"] ?? "");
        await EnrichContextFromScheduleAsync(context, schedule, db, ct);

        await SendAndLogAsync(schedule, user, context, db, renderer, provider, ct);

        schedule.SentAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    // ─── Bulk schedule ───────────────────────────────────────────────

    private async Task ProcessBulkAsync(
        EmailSchedule schedule, AppDbContext db,
        ILiquidRenderer renderer, IEmailProvider provider, CancellationToken ct)
    {
        var audience = await ResolveAudienceAsync(schedule.Template, db, ct);

        // Dedup: skip users who have already received this template
        var alreadySentTo = await db.EmailSendLogs
            .Where(l => l.TemplateId == schedule.TemplateId && l.Status == "Sent")
            .Select(l => l.UserId)
            .ToHashSetAsync(ct);

        var toSend = audience.Where(u => !alreadySentTo.Contains(u.Id)).ToList();

        _logger.LogInformation("Bulk schedule {Id} '{Name}': sending to {Count}/{Total} users",
            schedule.Id, schedule.Template.Name, toSend.Count, audience.Count);

        var logs = new List<EmailSendLog>();
        var weddingInfo = await db.WeddingInfo.FirstOrDefaultAsync(ct);

        foreach (var user in toSend)
        {
            try
            {
                var context = WeddingApp_Test.Infrastructure.Email.EmailDispatchService.BuildBaseContext(user, weddingInfo, _config["EmailConfig:ApiBaseUrl"] ?? "");
                // No per-reservation context for bulk schedules (context = null)

                var html    = await renderer.RenderAsync(schedule.Template.HtmlBody, context, ct);
                var subject = await renderer.RenderAsync(schedule.Template.Subject,   context, ct);

                await provider.SendAsync(user.Email, subject, html, ct);

                logs.Add(new EmailSendLog {
                    Id = Guid.NewGuid(), TemplateId = schedule.TemplateId,
                    ScheduleId = schedule.Id, UserId = user.Id,
                    SentAt = DateTime.UtcNow, Status = "Sent"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk send failed for user {Email}", user.Email);
                logs.Add(new EmailSendLog {
                    Id = Guid.NewGuid(), TemplateId = schedule.TemplateId,
                    ScheduleId = schedule.Id, UserId = user.Id,
                    SentAt = DateTime.UtcNow, Status = "Failed",
                    Error = ex.Message[..Math.Min(ex.Message.Length, 1000)]
                });
            }
        }

        await db.EmailSendLogs.AddRangeAsync(logs, ct);
        schedule.SentAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    // ─── Audience resolver ───────────────────────────────────────────

    private static async Task<List<User>> ResolveAudienceAsync(
        EmailTemplate template, AppDbContext db, CancellationToken ct)
    {
        return template.AudienceType switch
        {
            AudienceType.All =>
                await db.Users.Where(u => u.Role != UserRole.Admin).ToListAsync(ct),

            AudienceType.Attending =>
                await db.Rsvps.Include(r => r.User)
                    .Where(r => r.IsAttending)
                    .Select(r => r.User).ToListAsync(ct),

            AudienceType.NotAttending =>
                await db.Rsvps.Include(r => r.User)
                    .Where(r => !r.IsAttending)
                    .Select(r => r.User).ToListAsync(ct),

            AudienceType.NoRsvp =>
                await db.Users
                    .Where(u => u.Role != UserRole.Admin
                             && !db.Rsvps.Any(r => r.UserId == u.Id))
                    .ToListAsync(ct),

            AudienceType.NoGiftReservation =>
                await db.Users
                    .Where(u => u.Role != UserRole.Admin
                             && !db.GiftReservations.Any(gr => gr.ReservedByUserId == u.Id))
                    .ToListAsync(ct),

            AudienceType.HasGiftReservation =>
                await db.GiftReservations.Include(gr => gr.ReservedBy)
                    .Select(gr => gr.ReservedBy).Distinct().ToListAsync(ct),

            AudienceType.ByRole =>
                await db.Users.Where(u => u.Role == template.TargetRole).ToListAsync(ct),

            AudienceType.TriggeredUser =>
                [], // TriggeredUser is only for OnEvent — shouldn't appear on bulk schedules

            _ => throw new ArgumentOutOfRangeException(nameof(template.AudienceType))
        };
    }

    // ─── Context enrichment from schedule JSON ───────────────────────

    private static async Task EnrichContextFromScheduleAsync(
        Dictionary<string, object?> context, EmailSchedule schedule, AppDbContext db, CancellationToken ct)
    {
        if (schedule.Context is null) return;

        var extra = JsonSerializer.Deserialize<Dictionary<string, string>>(schedule.Context);
        if (extra is null) return;

        if (extra.TryGetValue("GiftId", out var giftIdStr) && Guid.TryParse(giftIdStr, out var giftId))
        {
            var gift = await db.Gifts.FindAsync([giftId], ct);
            if (gift is not null)
                context["gift"] = new { name = gift.Name, price = gift.Price, purchaseLink = gift.PurchaseLink };
        }

        if (extra.TryGetValue("GiftReservationId", out var resIdStr) && Guid.TryParse(resIdStr, out var resId))
        {
            var reservation = await db.GiftReservations.FindAsync([resId], ct);
            if (reservation is not null)
                context["reservation"] = new { reservedAt = reservation.ReservedAt.ToString("dd MMM yyyy"), reservation.Notes };
        }

        if (extra.TryGetValue("RsvpId", out var rsvpIdStr) && Guid.TryParse(rsvpIdStr, out var rsvpId))
        {
            var rsvp = await db.Rsvps.Include(r => r.Companions).FirstOrDefaultAsync(r => r.Id == rsvpId, ct);
            if (rsvp is not null)
                context["rsvp"] = new { isAttending = rsvp.IsAttending, companionCount = rsvp.Companions.Count };
        }
    }

    // ─── Send + log helper ───────────────────────────────────────────

    private static async Task SendAndLogAsync(
        EmailSchedule schedule, User user,
        Dictionary<string, object?> context,
        AppDbContext db, ILiquidRenderer renderer, IEmailProvider provider, CancellationToken ct)
    {
        var html    = await renderer.RenderAsync(schedule.Template.HtmlBody, context, ct);
        var subject = await renderer.RenderAsync(schedule.Template.Subject,   context, ct);

        await provider.SendAsync(user.Email, subject, html, ct);

        db.EmailSendLogs.Add(new EmailSendLog
        {
            Id = Guid.NewGuid(),
            TemplateId = schedule.TemplateId,
            ScheduleId = schedule.Id,
            UserId = user.Id,
            SentAt = DateTime.UtcNow,
            Status = "Sent"
        });
    }
}
