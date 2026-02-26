using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.WeddingInfo;

public class WeddingInfoDto
{
    // Visible to anyone
    public UserRole? UserRole { get; set; } // can be null if user authenticated
    public string BrideName { get; set; } = string.Empty;
    public string BrideSurname { get; set; } = string.Empty;
    public string GroomName { get; set; } = string.Empty;
    public string GroomSurname { get; set; } = string.Empty;
    public string ApproximateDate { get; set; } = string.Empty; // ex.: Summer 2027
    public string WeddingName { get; set; } = string.Empty;
    public string WeddingDescription { get; set; } = string.Empty;
    
    // Visible to Authenticated users
    public DateTime? WeddingDate { get; set; }
    public LocationDto? LocationCivil { get; set; }
    public LocationDto? LocationChurch { get; set; }
    
    // Visible to Full + Admin
    public LocationDto? LocationParty { get; set; }
    // livestreamUrl
    
    // Admin only
    public LocationDto? LocationHouse { get; set; } // TODO(REMOVE) only for testing
}

