namespace WeddingApp_Test.Application.DTO.Rsvp;

public class RsvpDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool IsAttending { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    public List<GuestCompanionDto> Companions { get; set; } = [];
    
    // Main guest's dietary restrictions
    public string? DietaryRestrictions { get; set; }
    public string? Notes { get; set; }
    public bool WantsReminder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed property
    public int TotalGuests => 1 + Companions.Count;
    
    // User's max allowed companions
    public int MaxCompanionsAllowed { get; set; }
}