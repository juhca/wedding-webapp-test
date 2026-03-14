using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// Tracks individual reservations of a gift
/// One user can reserve one gift only once (enforced by unique constraint)
/// </summary>
public class GiftReservation
{
    public Guid Id { get; set; }
    
    // Foreign Keys
    public Guid GiftId { get; set; }
    public Gift Gift { get; set; } = null!;
    
    public Guid ReservedByUserId { get; set; }
    public User ReservedBy { get; set; } = null!;
    
    // Reservation details
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Reminder system
    public bool ReminderRequested { get; set; }
    public DateTime? ReminderScheduledFor { get; set; }
    public DateTime? ReminderSentAt { get; set; }
}