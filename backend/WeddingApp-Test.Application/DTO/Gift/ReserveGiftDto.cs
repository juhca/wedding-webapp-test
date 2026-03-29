using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Gift;

public class ReserveGiftDto
{
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [EmailAddress, MaxLength(255)] 
    public string? Email { get; set; } = string.Empty;
}