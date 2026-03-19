using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// Handles OnEvent email triggers — fires immediately when a domain event occurs.
/// Looks up matching active templates, deduplicates via EmailSendLog, renders Liquid, and sends.
/// </summary>
public class EmailDispatchService : IEmailDispatchService
{
    private readonly AppDbContext _db;
    private readonly IEmailProvider _provider;
    private readonly ILiquidRenderer _renderer;
    private readonly ILogger<EmailDispatchService> _logger;
    private readonly string _apiBaseUrl;

    public EmailDispatchService(
        AppDbContext db,
        IEmailProvider provider,
        ILiquidRenderer renderer,
        IConfiguration config,
        ILogger<EmailDispatchService> logger)
    {
        _db = db;
        _provider = provider;
        _renderer = renderer;
        _logger = logger;
        _apiBaseUrl = config["EmailConfig:ApiBaseUrl"] ?? "https://localhost:5001";
    }

    public async Task DispatchEventAsync(
        string eventName,
        User triggeringUser,
        Dictionary<string, object?> extraContext,
        CancellationToken ct = default)
    {
        var templates = await _db.EmailTemplates
            .Where(t => t.IsActive
                     && t.TriggerType == TriggerType.OnEvent
                     && t.EventName == eventName)
            .ToListAsync(ct);

        if (templates.Count == 0) return;

        var weddingInfo = await _db.WeddingInfo.FirstOrDefaultAsync(ct);

        foreach (var template in templates)
        {
            // Dedup: skip if already sent to this user (MaxSendsPerUser = 1)
            if (template.MaxSendsPerUser.HasValue)
            {
                var sentCount = await _db.EmailSendLogs
                    .CountAsync(l => l.TemplateId == template.Id
                                  && l.UserId == triggeringUser.Id
                                  && l.Status == "Sent", ct);

                if (sentCount >= template.MaxSendsPerUser.Value)
                {
                    _logger.LogDebug("Template {Name} already sent to {UserId} — skipping", template.Name, triggeringUser.Id);
                    continue;
                }
            }

            try
            {
                var context = BuildBaseContext(triggeringUser, weddingInfo, _apiBaseUrl);
                foreach (var (k, v) in extraContext) context[k] = v;

                var html    = await _renderer.RenderAsync(template.HtmlBody, context, ct);
                var subject = await _renderer.RenderAsync(template.Subject,   context, ct);

                await _provider.SendAsync(triggeringUser.Email, subject, html, ct);

                _db.EmailSendLogs.Add(new EmailSendLog
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    ScheduleId = null,
                    UserId = triggeringUser.Id,
                    SentAt = DateTime.UtcNow,
                    Status = "Sent"
                });

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Dispatched '{Event}' email '{Template}' to {Email}",
                    eventName, template.Name, triggeringUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch '{Event}' template '{Template}' to {Email}",
                    eventName, template.Name, triggeringUser.Email);

                _db.EmailSendLogs.Add(new EmailSendLog
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    UserId = triggeringUser.Id,
                    SentAt = DateTime.UtcNow,
                    Status = "Failed",
                    Error = ex.Message[..Math.Min(ex.Message.Length, 1000)]
                });

                try { await _db.SaveChangesAsync(ct); } catch { /* best effort */ }
            }
        }
    }

    public static Dictionary<string, object?> BuildBaseContext(User user, WeddingInfo? wedding, string apiBaseUrl = "")
    {
        var token = Guid.NewGuid().ToString("N"); // unique per email = cache busting for Gmail
        return new Dictionary<string, object?>
        {
            ["user"] = new
            {
                firstName = user.FirstName,
                lastName  = user.LastName,
                email     = user.Email
            },
            ["wedding"] = wedding is null ? null : new
            {
                name              = wedding.WeddingName,
                date              = wedding.WeddingDate?.ToString("dddd, dd MMMM yyyy"),
                ceremonyLocation  = wedding.ChurchLocationName,
                civilLocation     = wedding.CivilLocationName,
                partyLocation     = wedding.PartyLocationName
            },
            ["countdown"] = wedding?.WeddingDate is null ? null : new
            {
                days      = Math.Max(0, (int)(wedding.WeddingDate.Value.Date - DateTime.UtcNow.Date).TotalDays),
                hours     = (int)(wedding.WeddingDate.Value - DateTime.UtcNow).TotalHours % 24,
                imageUrl  = string.IsNullOrEmpty(apiBaseUrl)
                              ? null
                              : $"{apiBaseUrl}/api/Email/countdown?token={token}",
                guestImageUrl = string.IsNullOrEmpty(apiBaseUrl)
                              ? null
                              : $"{apiBaseUrl}/api/Email/guest-message?guestId={user.Id}&cb={token}"
            }
        };
    }
}
