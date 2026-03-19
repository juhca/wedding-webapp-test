using System.ComponentModel.DataAnnotations;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// Defines the blueprint for an email: content (Liquid template), when to trigger it, and who receives it.
/// Default templates are seeded via DataSeeder. Admin can create/edit additional templates at runtime.
/// </summary>
public class EmailTemplate
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string HtmlBody { get; set; } = string.Empty;

    // ── Trigger ──────────────────────────────────────────────────────

    public TriggerType TriggerType { get; set; }

    /// <summary>
    /// For OnEvent triggers: the event name that fires this template.
    /// e.g. "RsvpSubmitted", "GiftReserved", "GiftUnreserved"
    /// </summary>
    [MaxLength(100)]
    public string? EventName { get; set; }

    /// <summary>
    /// For ScheduledRelative: offset in days from <see cref="RelativeTo"/>.
    /// Negative = before the reference date (e.g. -7 = 7 days before wedding).
    /// Positive = after (e.g. +7 = 7 days after wedding).
    /// </summary>
    public int? OffsetDays { get; set; }

    /// <summary>
    /// For ScheduledRelative: the reference date to offset from.
    /// e.g. "WeddingDate", "RsvpDate", "GiftReservationDate"
    /// </summary>
    [MaxLength(50)]
    public string? RelativeTo { get; set; }

    /// <summary>
    /// For ScheduledAbsolute: the exact UTC date to send on.
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    // ── Audience ─────────────────────────────────────────────────────

    public AudienceType AudienceType { get; set; }

    /// <summary>
    /// Used when AudienceType = ByRole. Null = any role.
    /// </summary>
    public UserRole? TargetRole { get; set; }

    /// <summary>
    /// How many times this template may be sent to the same user.
    /// 1 = once only; null = unlimited.
    /// </summary>
    public int? MaxSendsPerUser { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────

    public List<EmailSchedule> Schedules { get; set; } = [];
    public List<EmailSendLog> SendLogs { get; set; } = [];
}
