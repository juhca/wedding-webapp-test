using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Rsvp;

public class CreateRsvpDto
{
    [Required]
    public bool IsAttending { get; set; }
    
    public List<CreateGuestCompanionDto> Companions { get; set; } = [];
    
    // Main guest's dietary restrictions
    [MaxLength(500)]
    public string? DietaryRestrictions { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }

    [EmailAddress, MaxLength(255)]
    public string? RecipientEmail { get; set; }
}