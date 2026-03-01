namespace WeddingApp_Test.Application.DTO.Rsvp;

public class CateringExportDto
{
    public string GuestType { get; set; } = string.Empty; // "Main" or "Companion"
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? DietaryRestrictions { get; set; }
    public string? Notes { get; set; }
    public string MainGuestEmail { get; set; } = string.Empty; // Reference to main guest
}