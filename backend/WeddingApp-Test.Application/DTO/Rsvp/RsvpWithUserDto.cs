namespace WeddingApp_Test.Application.DTO.Rsvp;

public class RsvpWithUserDto : RsvpDto
{
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}