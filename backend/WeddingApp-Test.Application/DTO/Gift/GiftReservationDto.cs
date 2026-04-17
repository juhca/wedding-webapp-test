using GiftReservationEntity = WeddingApp_Test.Domain.Entities.GiftReservation;

namespace WeddingApp_Test.Application.DTO.Gift;

public class GiftReservationDto
{
    public Guid Id { get; set; }
    public Guid GiftId { get; set; }
    public Guid ReservedByUserId { get; set; }
    public string ReservedByName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public string? Notes { get; set; }

    public static GiftReservationDto FromEntity(GiftReservationEntity r) => new()
    {
        Id = r.Id,
        GiftId = r.GiftId,
        ReservedByUserId = r.ReservedByUserId,
        ReservedAt = r.ReservedAt,
        Notes = r.Notes,
        ReservedByName = $"{r.ReservedBy.FirstName} {r.ReservedBy.LastName}"
    };
}