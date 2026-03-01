using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// Companion/guest that a user is bringing to the wedding
/// </summary>
public class GuestCompanion
{
    public Guid Id { get; set; }
    
    // Foreign key to RSVP
    public Guid RsvpId { get; set; }
    public Rsvp Rsvp { get; set; } = null!;
    
    // Companion details
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Age of companion (useful for seating, catering)
    /// </summary>
    public int? Age { get; set; }
    
    /// <summary>
    /// Dietary restrictions for this specific companion
    /// </summary>
    [MaxLength(500)]
    public string? DietaryRestrictions { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}