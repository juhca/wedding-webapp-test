using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class WeddingInfoService : IWeddingInfoService
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
            WeddingDescription = "Wedding Day Description"
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
            GoogleMapsUrl = "https://maps.google.com/?q=46.0503,14.5069"
        };
        
        info.LocationChurch = new LocationDto()
        {
            Name = "St. Nicholas Cathedral",
            Address = "Dolničarjeva ulica 1, 1000 Ljubljana, Slovenia",
            Latitude = 46.0512,
            Longitude = 14.5082,
            GoogleMapsUrl = "https://maps.google.com/?q=46.0512,14.5082"
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
            GoogleMapsUrl = "https://maps.google.com/?q=46.0546,14.5066"
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
            GoogleMapsUrl = "https://maps.google.com/?q=46.0522,14.5155"
        };
            
        return info;
    }
}