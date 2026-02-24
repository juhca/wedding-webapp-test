using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class WeddingInfoService : IWeddingInfoService
{
    public async Task<WeddingInfoDto> GetWeddingInfoAsync(UserRole? userRole)
    {
        var info = new WeddingInfoDto(
            new DateTime(2027, 6, 19, 12, 45, 0), 
            "Jane & Joe Wedding Day", 
            "Wedding description");
        
        return info;
    }
}