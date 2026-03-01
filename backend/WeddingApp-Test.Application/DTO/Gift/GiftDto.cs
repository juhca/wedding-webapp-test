namespace WeddingApp_Test.Application.DTO.Gift;

public class GiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? PurchaseLink { get; set; }
    
    // Reservation info
    public int? MaxReservations { get; set; }
    public int ReservationCount { get; set; }
    public int? RemainingReservations { get; set; }
    public bool IsFullyReserved { get; set; }
    
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    
    // User-specific
    public bool IsReservedByMe { get; set; }
    public List<GiftReservationDto> Reservations { get; set; } = [];
    
    // Display helper
    public string ReservationStatus
    {
        get
        {
            if (!MaxReservations.HasValue)
                return $"{ReservationCount} reserved (unlimited)";
            
            if (IsFullyReserved)
                return "Fully reserved";
            
            return $"{ReservationCount}/{MaxReservations} reserved";
        }
    }
}