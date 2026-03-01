using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Gift;

public class UpdateGiftDto
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
    
    [Range(1, 100)]
    public int? MaxReservations { get; set; }
    
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
}