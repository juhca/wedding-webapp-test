namespace WeddingApp_Test.Application.DTO.Gift;

public class GiftReservationConfirmationDto
{
    public Guid ReservationId { get; set; }
    public Guid GiftId { get; set; }
    public string GiftName { get; set; } = string.Empty;
    public string? PurchaseLink { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingReservations { get; set; }
    public bool GiftFullyReserved { get; set; }
}