namespace WeddingApp_Test.Application.DTO.Rsvp;

public class RsvpSummaryDto
{
    public int TotalInvited { get; set; }
    public int TotalResponded { get; set; }
    public int TotalAttending { get; set; }
    public int TotalNotAttending { get; set; }
    
    public int TotalPeople { get; set; } // Self + all companions
    public int TotalCompanions { get; set; } // Just companions
    
    public int PendingResponses { get; set; }
    public List<RsvpWithUserDto> AttendingGuests { get; set; } = [];
    public List<RsvpWithUserDto> NotAttendingGuests { get; set; } = [];
    public List<RsvpWithUserDto> PendingGuests { get; set; } = [];
}
