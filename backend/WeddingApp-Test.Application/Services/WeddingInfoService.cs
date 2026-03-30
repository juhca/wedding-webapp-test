using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class WeddingInfoService(IWeddingInfoRepository weddingInfoRepository) : IWeddingInfoService
{
    public async Task<WeddingInfoDto?> GetWeddingInfoAsync(UserRole? userRole)
    {
        var weddingInfo = await weddingInfoRepository.GetWeddingInfoAsync();

        if (weddingInfo is null)
        {
            return null;
        }
        
        var info = new WeddingInfoDto
        {
            ApproximateDate = weddingInfo.ApproximateDate,
            BrideName = weddingInfo.BrideName,
            BrideSurname = weddingInfo.BrideSurname,
            GroomName = weddingInfo.GroomName,
            GroomSurname = weddingInfo.GroomSurname,
            WeddingName = weddingInfo.WeddingName,
            WeddingDescription = weddingInfo.WeddingDescription,
            UserRole = userRole
        };

        if (userRole is null)
        {
            return info;
        }
        
        info.WeddingDate = weddingInfo.WeddingDate;
        info.LocationCivil = new LocationDto()
        {
            Name = weddingInfo.CivilLocationName,
            Address = weddingInfo.CivilLocationAddress,
            Latitude = weddingInfo.CivilLocationLatitude,
            Longitude = weddingInfo.CivilLocationLongitude,
            GoogleMapsUrl = weddingInfo.CivilLocationGoogleMapsUrl,
            AppleMapsUrl = weddingInfo.CivilLocationAppleMapsUrl
        };
        
        info.LocationChurch = new LocationDto()
        {
            Name = weddingInfo.ChurchLocationName,
            Address = weddingInfo.ChurchLocationAddress,
            Latitude = weddingInfo.ChurchLocationLatitude,
            Longitude = weddingInfo.ChurchLocationLongitude,
            GoogleMapsUrl = weddingInfo.ChurchLocationGoogleMapsUrl,
            AppleMapsUrl = weddingInfo.ChurchLocationAppleMapsUrl
        };

        if (userRole == UserRole.LimitedExperience)
        {
            return info;
        }
        
        info.LocationParty = new LocationDto()
        {
            Name = weddingInfo.PartyLocationName,
            Address = weddingInfo.PartyLocationAddress,
            Latitude = weddingInfo.PartyLocationLatitude,
            Longitude = weddingInfo.PartyLocationLongitude,
            GoogleMapsUrl = weddingInfo.PartyLocationGoogleMapsUrl,
            AppleMapsUrl = weddingInfo.PartyLocationAppleMapsUrl
        };
    
        if (userRole == UserRole.FullExperience)
        {
            return info;
        }
        
        // Only admin can receive this
        info.LocationHouse = new LocationDto()
        {
            Name = weddingInfo.HouseLocationName,
            Address = weddingInfo.HouseLocationAddress,
            Latitude = weddingInfo.HouseLocationLatitude,
            Longitude = weddingInfo.HouseLocationLongitude,
            GoogleMapsUrl = weddingInfo.HouseLocationGoogleMapsUrl,
            AppleMapsUrl = weddingInfo.HouseLocationAppleMapsUrl
        };
            
        return info;
    }

    public async Task<WeddingInfoDto?> UpdateWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid updatedByUserId)
    {
        var weddingInfo = await weddingInfoRepository.GetWeddingInfoAsync();
        
        if (weddingInfo is null)
        {
            throw new InvalidOperationException("Wedding info not initialized. Call InitializeWeddingInfoAsync first.");
        }
        ApplyDto(dto, weddingInfo);
        weddingInfo.UpdatedByUserId = updatedByUserId;
        await weddingInfoRepository.UpdateAsync(weddingInfo);
        
        return await GetWeddingInfoAsync(UserRole.Admin);
    }

    public async Task<WeddingInfoDto?> InitializeWeddingInfoAsync(WeddingInfoUpdateDto dto, Guid createdByUserId)
    {
        var exists = await weddingInfoRepository.ExistsAsync();
        if (exists)
        {
            throw new InvalidOperationException("Wedding info already exists. Use UpdateWeddingInfoAsync instead.");
        }
        
        var weddingInfo = new WeddingInfo();
        ApplyDto(dto, weddingInfo);
        weddingInfo.Id = Guid.NewGuid();
        weddingInfo.UpdatedByUserId = createdByUserId;
        
        await weddingInfoRepository.CreateAsync(weddingInfo);
        
        return await GetWeddingInfoAsync(UserRole.Admin);
    }

    private static void ApplyDto(WeddingInfoUpdateDto dto, WeddingInfo entity)
    {
        entity.BrideName = dto.BrideName;
        entity.BrideSurname = dto.BrideSurname;
        entity.GroomName = dto.GroomName;
        entity.GroomSurname = dto.GroomSurname;
        entity.ApproximateDate = dto.ApproximateDate;
        entity.WeddingName = dto.WeddingName;
        entity.WeddingDescription = dto.WeddingDescription;
        entity.WeddingDate = dto.WeddingDate;
        entity.CivilLocationName = dto.CivilLocationName;
        entity.CivilLocationAddress = dto.CivilLocationAddress;
        entity.CivilLocationLatitude = dto.CivilLocationLatitude;
        entity.CivilLocationLongitude = dto.CivilLocationLongitude;
        entity.CivilLocationGoogleMapsUrl = dto.CivilLocationGoogleMapsUrl;
        entity.CivilLocationAppleMapsUrl = dto.CivilLocationAppleMapsUrl;
        entity.ChurchLocationName = dto.ChurchLocationName;
        entity.ChurchLocationAddress = dto.ChurchLocationAddress;
        entity.ChurchLocationLatitude = dto.ChurchLocationLatitude;
        entity.ChurchLocationLongitude = dto.ChurchLocationLongitude;
        entity.ChurchLocationGoogleMapsUrl = dto.ChurchLocationGoogleMapsUrl;
        entity.ChurchLocationAppleMapsUrl = dto.ChurchLocationAppleMapsUrl;
        entity.PartyLocationName = dto.PartyLocationName;
        entity.PartyLocationAddress = dto.PartyLocationAddress;
        entity.PartyLocationLatitude = dto.PartyLocationLatitude;
        entity.PartyLocationLongitude = dto.PartyLocationLongitude;
        entity.PartyLocationGoogleMapsUrl = dto.PartyLocationGoogleMapsUrl;
        entity.PartyLocationAppleMapsUrl = dto.PartyLocationAppleMapsUrl;
        entity.LivestreamUrl = dto.LivestreamUrl;
        entity.HouseLocationName = dto.HouseLocationName;
        entity.HouseLocationAddress = dto.HouseLocationAddress;
        entity.HouseLocationLatitude = dto.HouseLocationLatitude;
        entity.HouseLocationLongitude = dto.HouseLocationLongitude;
        entity.HouseLocationGoogleMapsUrl = dto.HouseLocationGoogleMapsUrl;
        entity.HouseLocationAppleMapsUrl = dto.HouseLocationAppleMapsUrl;
    }
}