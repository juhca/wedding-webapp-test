using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Interfaces;

public interface IWeddingInfoService
{
    public Task<WeddingInfoDto> GetWeddingInfoAsync(UserRole? userRole);
}