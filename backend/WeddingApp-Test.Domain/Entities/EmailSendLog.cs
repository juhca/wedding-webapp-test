using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// Audit record written after each individual email send attempt.
/// Used for deduplication (MaxSendsPerUser enforcement) and debugging.
/// </summary>
public class EmailSendLog
{
    public Guid Id { get; set; }

    // Foreign Keys
    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;

    public Guid? ScheduleId { get; set; }
    public EmailSchedule? Schedule { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime SentAt { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Sent"; // "Sent" | "Failed"

    [MaxLength(1000)]
    public string? Error { get; set; }
}
