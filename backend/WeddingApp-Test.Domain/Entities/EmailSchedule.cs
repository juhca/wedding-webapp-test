namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// A specific scheduled send instance.
/// 
/// Per-user (UserId set): created automatically when a user triggers a reminder 
///   (e.g. gift reservation with WantsReminder=true).
/// 
/// Bulk (UserId = null): a single row drives a send to a dynamically resolved audience
///   (e.g. "remind all guests with no gift reservation on 2025-08-30").
/// </summary>
public class EmailSchedule
{
    public Guid Id { get; set; }

    // Foreign Key
    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;

    /// <summary>
    /// Null = bulk schedule (audience resolved at send time from Template.AudienceType).
    /// Set = per-user schedule for a specific guest.
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public DateTime ScheduledFor { get; set; }

    /// <summary>
    /// Set when the schedule has been processed. Null = still pending.
    /// For bulk schedules this is set after ALL users in the audience have been processed.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// JSON payload with entity IDs needed to build the Liquid data context.
    /// e.g. { "GiftId": "...", "GiftReservationId": "..." } or { "RsvpId": "..." }
    /// Null for bulk schedules (context is built from user + wedding data only).
    /// </summary>
    public string? Context { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<EmailSendLog> SendLogs { get; set; } = [];
}
