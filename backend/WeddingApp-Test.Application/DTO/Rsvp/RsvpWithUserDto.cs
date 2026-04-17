using RsvpEntity = WeddingApp_Test.Domain.Entities.Rsvp;

namespace WeddingApp_Test.Application.DTO.Rsvp;

public class RsvpWithUserDto : RsvpDto
{
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public new static RsvpWithUserDto FromEntity(RsvpEntity r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        IsAttending = r.IsAttending,
        RespondedAt = r.RespondedAt,
        DietaryRestrictions = r.DietaryRestrictions,
        Notes = r.Notes,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Companions = r.Companions.Select(GuestCompanionDto.FromEntity).ToList(),
        MaxCompanionsAllowed = r.User?.MaxCompanions ?? 0,
        UserFirstName = r.User?.FirstName ?? "",
        UserLastName = r.User?.LastName ?? "",
        UserEmail = r.User?.Email ?? ""
    };
}