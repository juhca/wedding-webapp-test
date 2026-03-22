using System.ComponentModel.DataAnnotations;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.Reminder;

public class AddReminderDto
{
    [Required, Range(1, 365)]
    public int Value { get; set; }

    [Required]
    public ReminderUnit Unit { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
