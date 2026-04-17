using System.ComponentModel.DataAnnotations;
using WeddingInfoEntity = WeddingApp_Test.Domain.Entities.WeddingInfo;

namespace WeddingApp_Test.Application.DTO.WeddingInfo;

public class WeddingInfoUpdateDto
{
    // Public info
    [Required, MaxLength(100)]
    public string BrideName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string BrideSurname { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string GroomName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string GroomSurname { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string ApproximateDate { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string WeddingName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string WeddingDescription { get; set; } = string.Empty;
    
    // Authenticated info
    public DateTime? WeddingDate { get; set; }
    
    // Civil location
    [MaxLength(200)]
    public string? CivilLocationName { get; set; }
    
    [MaxLength(500)]
    public string? CivilLocationAddress { get; set; }
    
    public double? CivilLocationLatitude { get; set; }
    public double? CivilLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? CivilLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? CivilLocationAppleMapsUrl { get; set; }
    
    // Church location
    [MaxLength(200)]
    public string? ChurchLocationName { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationAddress { get; set; }
    
    public double? ChurchLocationLatitude { get; set; }
    public double? ChurchLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? ChurchLocationAppleMapsUrl { get; set; }
    
    // Party location (Full + Admin)
    [MaxLength(200)]
    public string? PartyLocationName { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationAddress { get; set; }
    
    public double? PartyLocationLatitude { get; set; }
    public double? PartyLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? PartyLocationAppleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? LivestreamUrl { get; set; }
    
    // House location (Admin only)
    [MaxLength(200)]
    public string? HouseLocationName { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationAddress { get; set; }
    
    public double? HouseLocationLatitude { get; set; }
    public double? HouseLocationLongitude { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationGoogleMapsUrl { get; set; }
    
    [MaxLength(500)]
    public string? HouseLocationAppleMapsUrl { get; set; }

    public WeddingInfoEntity ToEntity() => new()
    {
        BrideName = BrideName,
        BrideSurname = BrideSurname,
        GroomName = GroomName,
        GroomSurname = GroomSurname,
        ApproximateDate = ApproximateDate,
        WeddingName = WeddingName,
        WeddingDescription = WeddingDescription,
        WeddingDate = WeddingDate,
        CivilLocationName = CivilLocationName,
        CivilLocationAddress = CivilLocationAddress,
        CivilLocationLatitude = CivilLocationLatitude,
        CivilLocationLongitude = CivilLocationLongitude,
        CivilLocationGoogleMapsUrl = CivilLocationGoogleMapsUrl,
        CivilLocationAppleMapsUrl = CivilLocationAppleMapsUrl,
        ChurchLocationName = ChurchLocationName,
        ChurchLocationAddress = ChurchLocationAddress,
        ChurchLocationLatitude = ChurchLocationLatitude,
        ChurchLocationLongitude = ChurchLocationLongitude,
        ChurchLocationGoogleMapsUrl = ChurchLocationGoogleMapsUrl,
        ChurchLocationAppleMapsUrl = ChurchLocationAppleMapsUrl,
        PartyLocationName = PartyLocationName,
        PartyLocationAddress = PartyLocationAddress,
        PartyLocationLatitude = PartyLocationLatitude,
        PartyLocationLongitude = PartyLocationLongitude,
        PartyLocationGoogleMapsUrl = PartyLocationGoogleMapsUrl,
        PartyLocationAppleMapsUrl = PartyLocationAppleMapsUrl,
        LivestreamUrl = LivestreamUrl,
        HouseLocationName = HouseLocationName,
        HouseLocationAddress = HouseLocationAddress,
        HouseLocationLatitude = HouseLocationLatitude,
        HouseLocationLongitude = HouseLocationLongitude,
        HouseLocationGoogleMapsUrl = HouseLocationGoogleMapsUrl,
        HouseLocationAppleMapsUrl = HouseLocationAppleMapsUrl
    };

    public void ApplyTo(WeddingInfoEntity w)
    {
        w.BrideName = BrideName;
        w.BrideSurname = BrideSurname;
        w.GroomName = GroomName;
        w.GroomSurname = GroomSurname;
        w.ApproximateDate = ApproximateDate;
        w.WeddingName = WeddingName;
        w.WeddingDescription = WeddingDescription;
        w.WeddingDate = WeddingDate;
        w.CivilLocationName = CivilLocationName;
        w.CivilLocationAddress = CivilLocationAddress;
        w.CivilLocationLatitude = CivilLocationLatitude;
        w.CivilLocationLongitude = CivilLocationLongitude;
        w.CivilLocationGoogleMapsUrl = CivilLocationGoogleMapsUrl;
        w.CivilLocationAppleMapsUrl = CivilLocationAppleMapsUrl;
        w.ChurchLocationName = ChurchLocationName;
        w.ChurchLocationAddress = ChurchLocationAddress;
        w.ChurchLocationLatitude = ChurchLocationLatitude;
        w.ChurchLocationLongitude = ChurchLocationLongitude;
        w.ChurchLocationGoogleMapsUrl = ChurchLocationGoogleMapsUrl;
        w.ChurchLocationAppleMapsUrl = ChurchLocationAppleMapsUrl;
        w.PartyLocationName = PartyLocationName;
        w.PartyLocationAddress = PartyLocationAddress;
        w.PartyLocationLatitude = PartyLocationLatitude;
        w.PartyLocationLongitude = PartyLocationLongitude;
        w.PartyLocationGoogleMapsUrl = PartyLocationGoogleMapsUrl;
        w.PartyLocationAppleMapsUrl = PartyLocationAppleMapsUrl;
        w.LivestreamUrl = LivestreamUrl;
        w.HouseLocationName = HouseLocationName;
        w.HouseLocationAddress = HouseLocationAddress;
        w.HouseLocationLatitude = HouseLocationLatitude;
        w.HouseLocationLongitude = HouseLocationLongitude;
        w.HouseLocationGoogleMapsUrl = HouseLocationGoogleMapsUrl;
        w.HouseLocationAppleMapsUrl = HouseLocationAppleMapsUrl;
    }
}