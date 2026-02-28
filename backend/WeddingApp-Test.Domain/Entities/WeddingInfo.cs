using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Domain.Entities;

/// <summary>
/// Main wedding information - should only have ONE record in database
/// </summary>
public class WeddingInfo
{
    public Guid Id { get; set; }
    
    // ========================================
    // PUBLIC INFO (visible to everyone)
    // ========================================
    
    [Required, MaxLength(100)]
    public string BrideName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string BrideSurname { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string GroomName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string GroomSurname { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string ApproximateDate { get; set; } = string.Empty; // "Summer 2027"
    
    [MaxLength(200)]
    public string WeddingName { get; set; } = string.Empty; // "Ana & Marko's Wedding"
    
    [MaxLength(1000)]
    public string WeddingDescription { get; set; } = string.Empty;
    
    // ========================================
    // AUTHENTICATED INFO (Lite + Full + Admin)
    // ========================================
    
    public DateTime? WeddingDate { get; set; }
    
    // Civil Ceremony Location
    [MaxLength(200)]
    public string? CivilLocationName { get; set; }
    
    [MaxLength(500)]
    public string? CivilLocationAddress { get; set; }
    
    public double? CivilLocationLatitude { get; set; }
    
    public double? CivilLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? CivilLocationGoogleMapsUrl { get; set; }
        
    [MaxLength(500)]
    public string? CivilLocationAppleMapsUrl { get; set; }
    
    // Church Ceremony Location
    [MaxLength(200)]
    public string? ChurchLocationName { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationAddress { get; set; }
    
    public double? ChurchLocationLatitude { get; set; }
    
    public double? ChurchLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationAppleMapsUrl { get; set; }
    
    // ========================================
    // FULL EXPERIENCE INFO (Full + Admin)
    // ========================================
    
    // Party/Reception Location
    [MaxLength(200)]
    public string? PartyLocationName { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationAddress { get; set; }
    
    public double? PartyLocationLatitude { get; set; }
    
    public double? PartyLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationAppleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? LivestreamUrl { get; set; }
    
    // ========================================
    // ADMIN ONLY INFO
    // ========================================
    
    // House Location (for admin/testing purposes)
    [MaxLength(200)]
    public string? HouseLocationName { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationAddress { get; set; }
    
    public double? HouseLocationLatitude { get; set; }
    
    public double? HouseLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationAppleMapsUrl { get; set; }
    
    // ========================================
    // METADATA
    // ========================================
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public User? UpdatedBy { get; set; }
}