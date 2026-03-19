using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController(AppDbContext db, IConfiguration config) : ControllerBase
{
    /// <summary>
    /// Returns available reminder date options, computed from the wedding date.
    /// Only returns options where the calculated date is in the future.
    /// </summary>
    [HttpGet("options")]
    [ProducesResponseType(typeof(IEnumerable<ReminderOptionDto>), 200)]
    public IActionResult GetOptions()
    {
        var weddingInfo = db.WeddingInfo.FirstOrDefault();
        if (weddingInfo?.WeddingDate is null)
            return Ok(Array.Empty<ReminderOptionDto>());

        var weddingDate = weddingInfo.WeddingDate.Value;
        var today = DateTime.UtcNow.Date;

        var beforeDays = config.GetSection("ReminderOptions:BeforeWedding").Get<int[]>() ?? [7, 14, 30, 60];
        var afterDays  = config.GetSection("ReminderOptions:AfterWedding").Get<int[]>()  ?? [7, 14, 30];

        var options = new List<ReminderOptionDto>();

        foreach (var days in beforeDays)
        {
            var date = weddingDate.AddDays(-days);
            if (date.Date > today)
                options.Add(new ReminderOptionDto(
                    Label: FormatLabel(days, before: true),
                    OffsetDays: -days,
                    Date: date.Date));
        }

        foreach (var days in afterDays)
        {
            var date = weddingDate.AddDays(days);
            options.Add(new ReminderOptionDto(
                Label: FormatLabel(days, before: false),
                OffsetDays: days,
                Date: date.Date));
        }

        return Ok(options.OrderBy(o => o.Date));
    }

    /// <summary>
    /// Creates a personal email reminder for the logged-in user based on an offset from the wedding date.
    /// </summary>
    [HttpPost("gift/{giftReservationId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateGiftReminder(Guid giftReservationId, [FromBody] CreateReminderDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var weddingInfo = await db.WeddingInfo.FirstOrDefaultAsync();
        if (weddingInfo?.WeddingDate is null)
            return BadRequest("Wedding date not set.");

        // Validate reservation belongs to this user
        var reservation = await db.GiftReservations
            .Include(gr => gr.Gift)
            .FirstOrDefaultAsync(gr => gr.Id == giftReservationId && gr.ReservedByUserId == userId);

        if (reservation is null)
            return NotFound("Gift reservation not found.");

        var reminderDate = weddingInfo.WeddingDate.Value.AddDays(dto.OffsetDays);

        // Before-wedding reminders must be in the future
        if (dto.OffsetDays < 0 && reminderDate.Date <= DateTime.UtcNow.Date)
            return BadRequest($"Reminder date {reminderDate:dd MMM yyyy} has already passed. Choose a later option.");

        // Find the gift reminder template
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.IsActive && t.EventName == "GiftReservedReminder");

        if (template is null)
            return BadRequest("Gift reminder email template is not configured.");

        // Avoid duplicate schedule for the same reservation
        var existingSchedule = await db.EmailSchedules
            .FirstOrDefaultAsync(s => s.TemplateId == template.Id
                                   && s.UserId == userId
                                   && s.SentAt == null
                                   && s.Context != null && s.Context.Contains(giftReservationId.ToString()));
        if (existingSchedule is not null)
        {
            existingSchedule.ScheduledFor = reminderDate;
        }
        else
        {
            db.EmailSchedules.Add(new Domain.Entities.EmailSchedule
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                UserId = userId,
                ScheduledFor = reminderDate,
                Context = JsonSerializer.Serialize(new
                {
                    GiftId = reservation.GiftId,
                    GiftReservationId = giftReservationId
                })
            });
        }

        await db.SaveChangesAsync();

        return Ok(new { message = $"Reminder set for {reminderDate:dd MMM yyyy}." });
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static string FormatLabel(int days, bool before)
    {
        var (count, unit) = days switch {
            7  => (1, "week"),
            14 => (2, "weeks"),
            30 => (1, "month"),
            60 => (2, "months"),
            _  => (days, "day" + (days == 1 ? "" : "s"))
        };

        return before
            ? $"{count} {unit} before the wedding"
            : $"{count} {unit} after the wedding";
    }
}

public record ReminderOptionDto(string Label, int OffsetDays, DateTime Date);
public record CreateReminderDto(int OffsetDays);
