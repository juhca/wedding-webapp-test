using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.DTO.Gift;

public class GiftReservationDto
{
    public Guid Id { get; set; }
    public Guid GiftId { get; set; }
    public Guid ReservedByUserId { get; set; }
    public string ReservedByName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public string? Notes { get; set; }
    public bool ReminderRequested { get; set; }
    public DateTime? ReminderScheduledFor { get; set; }

    public GiftReservationDto() { }

    public GiftReservationDto(GiftReservation r)
    {
        Id = r.Id;
        GiftId = r.GiftId;
        ReservedByUserId = r.ReservedByUserId;
        ReservedByName = $"{r.ReservedBy.FirstName} {r.ReservedBy.LastName}";
        ReservedAt = r.ReservedAt;
        Notes = r.Notes;
        ReminderRequested = r.ReminderRequested;
        ReminderScheduledFor = r.ReminderScheduledFor;
    }
}