namespace WeddingApp_Test.Application.DTO.Gift;

public class GiftReservationDto
{
    public Guid Id { get; set; }
    public Guid GiftId { get; set; }
    public Guid ReservedByUserId { get; set; }
    public string ReservedByName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public string? Notes { get; set; }
}