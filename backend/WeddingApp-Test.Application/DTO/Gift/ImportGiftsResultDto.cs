namespace WeddingApp_Test.Application.DTO.Gift;

public class ImportGiftsResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<GiftDto> ImportedGifts { get; set; } = [];
    public List<ImportGiftErrorDto> Errors { get; set; } = [];
}

public class ImportGiftErrorDto
{
    public int Row { get; set; }
    public string GiftName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}