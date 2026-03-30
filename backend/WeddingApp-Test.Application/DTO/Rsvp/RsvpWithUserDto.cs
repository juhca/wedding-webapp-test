using WeddingApp_Test.Domain.Entities;
using RsvpEntity = WeddingApp_Test.Domain.Entities.Rsvp;

namespace WeddingApp_Test.Application.DTO.Rsvp;

public class RsvpWithUserDto : RsvpDto
{
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public RsvpWithUserDto() { }

    public RsvpWithUserDto(RsvpEntity rsvp) : base(rsvp, rsvp.User?.MaxCompanions ?? 0)
    {
        UserFirstName = rsvp.User?.FirstName ?? string.Empty;
        UserLastName = rsvp.User?.LastName ?? string.Empty;
        UserEmail = rsvp.User?.Email ?? string.Empty;
    }
}