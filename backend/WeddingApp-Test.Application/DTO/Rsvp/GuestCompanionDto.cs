using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.DTO.Rsvp;

public class GuestCompanionDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Notes { get; set; }

    public GuestCompanionDto() { }

    public GuestCompanionDto(GuestCompanion c)
    {
        Id = c.Id;
        FirstName = c.FirstName;
        LastName = c.LastName;
        Age = c.Age;
        DietaryRestrictions = c.DietaryRestrictions;
        Notes = c.Notes;
    }
}