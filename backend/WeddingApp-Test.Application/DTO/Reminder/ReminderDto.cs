using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.Reminder;

public class ReminderDto
{
    public Guid Id { get; set; }
    public int Value { get; set; }
    public ReminderUnit Unit { get; set; }
    public string? Note { get; set; }
    public DateTime ScheduledFor { get; set; }
    public bool IsSent { get; set; }
    public DateTime CreatedAt { get; set; }
}
