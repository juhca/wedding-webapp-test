using AutoMapper;
using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class WeddingInfoService(IWeddingInfoRepository weddingInfoRepository, IMapper mapper) : IWeddingInfoService
{
    public async Task<WeddingInfoDto> GetWeddingInfoAsync(UserRole? userRole)
    {
        // TODO GET FROM SETTINGS/CONFIG/DB
        var info = new WeddingInfoDto
        {
            ApproximateDate = "Summer 2027",
            BrideName = "Jane",
            BrideSurname = "Doe",
            GroomName = "John",
            GroomSurname = "Toe",
            WeddingName = "Jane & Joe Wedding Day",
            WeddingDescription = "Wedding Day Description",
            UserRole = userRole
        };

        if (userRole is null)
        {
            return info;
        }
        
        info.WeddingDate = new DateTime(2027, 6, 19, 12, 45, 0);
        info.LocationCivil = new LocationDto()
        {
            Name = "Ljubljana City Hall",
            Address = "Mestni trg 1, 1000 Ljubljana, Slovenia",
            Latitude = 46.0503,
            Longitude = 14.5069,
            GoogleMapsUrl = "https://maps.google.com/?q=46.0503,14.5069",
            AppleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155"
        };
        
        info.LocationChurch = new LocationDto()
        {
            Name = "St. Nicholas Cathedral",
            Address = "Dolničarjeva ulica 1, 1000 Ljubljana, Slovenia",
            Latitude = 46.0512,
            Longitude = 14.5082,
            GoogleMapsUrl = "https://maps.google.com/?q=46.0512,14.5082",
            AppleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155"
        };

        if (userRole == UserRole.LimitedExperience)
        {
            return info;
        }
        
        info.LocationParty = new LocationDto()
        {
            Name = "Grand Hotel Union",
            Address = "Miklošičeva cesta 1, 1000 Ljubljana, Slovenia",
            Latitude = 46.0546,
            Longitude = 14.5066,
            GoogleMapsUrl = "https://maps.google.com/?q=46.0546,14.5066",
            AppleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155"
        };
    
        if (userRole == UserRole.FullExperience)
        {
            return info;
        }
        
        // Only admin can receive this
        info.LocationHouse = new LocationDto()
        {
            Name = "Bride's Family Home",
            Address = "Trubarjeva cesta 50, 1000 Ljubljana, Slovenia",
            Latitude = 46.0522,
            Longitude = 14.5155,
            GoogleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155",
            AppleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155"
        };
            
        return info;
    }

    public async Task<WeddingInfoDto> UpdateWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid updatedByUserId)
    {
        var weddingInfo = await weddingInfoRepository.GetWeddingInfoAsync();
        
        if (weddingInfo is null)
        {
            throw new InvalidOperationException("Wedding info not initialized. Call InitializeWeddingInfoAsync first.");
        }
        mapper.Map(dto, weddingInfo);
        weddingInfo.UpdatedByUserId = updatedByUserId;
        await weddingInfoRepository.UpdateAsync(weddingInfo);
        
        return await GetWeddingInfoAsync(UserRole.Admin);
    }

    public async Task<WeddingInfoDto> InitializeWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid createdByUserId)
    {
        var exists = await weddingInfoRepository.ExistsAsync();
        if (exists)
        {
            throw new InvalidOperationException("Wedding info already exists. Use UpdateWeddingInfoAsync instead.");
        }
        
        var weddingInfo = mapper.Map<WeddingInfo>(dto);
        weddingInfo.Id = Guid.NewGuid();
        weddingInfo.UpdatedByUserId = createdByUserId;
        
        await weddingInfoRepository.CreateAsync(weddingInfo);
        
        return await GetWeddingInfoAsync(UserRole.Admin);
    }
}