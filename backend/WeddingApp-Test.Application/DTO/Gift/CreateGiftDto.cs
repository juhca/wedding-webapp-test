using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Gift;

public class CreateGiftDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Range(0, 999999.99)]
    public decimal? Price { get; set; }
    
    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }
    
    [MaxLength(500)]
    [Url]
    public string? PurchaseLink { get; set; }
    
    /// <summary>
    /// Max reservations: 1 = single-use, >1 = multi-use, null = unlimited
    /// </summary>
    [Range(1, 100, ErrorMessage = "Max reservations must be between 1 and 100")]
    public int? MaxReservations { get; set; } = 1;
    
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
}