namespace WeddingApp_Test.Application.DTO.WeddingInfo;

public class WeddingInfoDto
{
    // Visible to anyone
    public string? BrideName { get; set; }
    public string? BrideSurname { get; set; }
    public string? GroomName { get; set; }
    public string? GroomSurname { get; set; }
    public string? ApproximateDate { get; set; } // ex.: Summer 2027
    public string? WeddingName { get; set; }
    public string? WeddingDescription { get; set; }
    
    // Visible to Authenticated users
    public DateTime WeddingDate { get; set; }
    public LocationDto? LocationCivil { get; set; }
    public LocationDto? LocationChurch { get; set; }
    // lokacija civilna + cerkvena
    
    // Visible to Full + Admin
    public LocationDto? LocationParty { get; set; }
    // livestreamUrl
    
    // Admin only
    public LocationDto? LocationHouse { get; set; } // TODO(REMOVE) only for testing
}

