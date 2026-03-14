using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Gift;

public class ReserveGiftDto
{
    public bool WantsReminder { get; set; } = false;
    
    public DateTime? ReminderDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}