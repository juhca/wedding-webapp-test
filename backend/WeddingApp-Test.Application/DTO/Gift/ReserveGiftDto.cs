using System.ComponentModel.DataAnnotations;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.Gift;

public class ReserveGiftDto
{
    public bool WantsReminder { get; set; } = false;

    public ReminderOffsetUnit? ReminderOffsetUnit { get; set; }

    [Range(1, 52, ErrorMessage = "ReminderOffsetValue must be between 1 and 52.")]
    public int? ReminderOffsetValue { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}