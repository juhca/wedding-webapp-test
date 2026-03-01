using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

public class Gift
{
    public Guid Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public decimal? Price { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(500)]
    public string? PurchaseLink { get; set; }
    
    /// <summary>
    /// Maximum number of times this gift can be reserved
    /// 1 = single-use (cutlery set, vase)
    /// >1 = multi-use (gift cards - e.g., 5 people can each give a $50 gift card)
    /// null = unlimited (cash gifts, honeymoon fund)
    /// </summary>
    public int? MaxReservations { get; set; } = 1;
    
    /// <summary>
    /// All reservations for this gift
    /// </summary>
    public List<GiftReservation> Reservations { get; set; } = [];
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Display
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    
    // COMPUTED PROPERTIES (don't save to DB)
    /// <summary>
    /// Number of current reservations
    /// </summary>
    public int ReservationCount => Reservations?.Count ?? 0;
    
    /// <summary>
    /// Is this gift fully reserved?
    /// </summary>
    public bool IsFullyReserved => MaxReservations.HasValue && ReservationCount >= MaxReservations.Value;
    
    /// <summary>
    /// How many more reservations are allowed?
    /// null = unlimited
    /// </summary>
    public int? RemainingReservations => MaxReservations.HasValue 
        ? Math.Max(0, MaxReservations.Value - ReservationCount) 
        : null;
}