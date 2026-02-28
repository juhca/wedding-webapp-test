using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Interfaces;

public interface IWeddingInfoService
{
    /// <summary>
    /// Get wedding info filtered by user role
    /// </summary>
    public Task<WeddingInfoDto?> GetWeddingInfoAsync(UserRole? userRole);
    
    /// <summary>
    /// Update wedding info (Admin only)
    /// </summary>
    Task<WeddingInfoDto?> UpdateWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid updatedByUserId);
    
    /// <summary>
    /// Initialize wedding info if not exists
    /// </summary>
    Task<WeddingInfoDto?> InitializeWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid createdByUserId);
}