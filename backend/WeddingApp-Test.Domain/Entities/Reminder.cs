using System.ComponentModel.DataAnnotations;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Domain.Entities;

public class Reminder
{
    public Guid Id { get; set; }

    public ReminderType Type { get; set; }

    /// <summary>
    /// Points to GiftReservation.Id when Type=Gift, Rsvp.Id when Type=Rsvp.
    /// Extensible: adding a new type only requires a new enum value.
    /// </summary>
    public Guid TargetId { get; set; }

    public int Value { get; set; }

    public ReminderUnit Unit { get; set; }

    /// <summary>
    /// Calculated as WeddingDate - (Value * Unit). Must be in the future at creation time.
    /// </summary>
    public DateTime ScheduledFor { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
