using WeddingApp_Test.Domain.Entities;
using RsvpEntity = WeddingApp_Test.Domain.Entities.Rsvp;

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

    public RsvpDto() { }

    public RsvpDto(RsvpEntity rsvp, int maxCompanionsAllowed = 0)
    {
        Id = rsvp.Id;
        UserId = rsvp.UserId;
        IsAttending = rsvp.IsAttending;
        RespondedAt = rsvp.RespondedAt;
        Companions = rsvp.Companions.Select(c => new GuestCompanionDto(c)).ToList();
        DietaryRestrictions = rsvp.DietaryRestrictions;
        Notes = rsvp.Notes;
        WantsReminder = rsvp.WantsReminder;
        CreatedAt = rsvp.CreatedAt;
        UpdatedAt = rsvp.UpdatedAt;
        MaxCompanionsAllowed = maxCompanionsAllowed;
    }
}