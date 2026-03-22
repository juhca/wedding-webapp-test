using System.ComponentModel.DataAnnotations;

namespace WeddingApp_Test.Application.DTO.Gift;

public class ReserveGiftDto
{
    [MaxLength(500)]
    public string? Notes { get; set; }
}