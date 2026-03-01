using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Rsvp;

public class CreateGuestCompanionDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
    public int? Age { get; set; }
    
    [MaxLength(500)]
    public string? DietaryRestrictions { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}