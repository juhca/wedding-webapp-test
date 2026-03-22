using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

public class Rsvp
{
    public Guid Id { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Attendance
    public bool IsAttending { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    public List<GuestCompanion> Companions { get; set; } = [];
    
    // Main guest's dietary restrictions
    [MaxLength(500)]
    public string? DietaryRestrictions { get; set; }
    
    // Like ~ I'm coming early, but also leaving soon
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Total number of people (self + companions)
    /// </summary>
    public int TotalGuests => 1 + (Companions?.Count ?? 0);
}