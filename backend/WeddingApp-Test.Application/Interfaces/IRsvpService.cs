using WeddingApp_Test.Application.DTO.Rsvp;

namespace WeddingApp_Test.Application.Interfaces;

public interface IRsvpService
{
    Task<RsvpDto?> GetUserRsvpAsync(Guid userId);
    Task<RsvpDto> CreateOrUpdateRsvpAsync(Guid userId, CreateRsvpDto dto);
    Task<RsvpSummaryDto> GetSummaryAsync();
    Task<IEnumerable<RsvpWithUserDto>> GetAllWithUsersAsync();
    Task<IEnumerable<CateringExportDto>> ExportForCateringAsync();
}