using AutoMapper;
using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // WeddingInfo Mappings
        CreateMap<WeddingInfo, WeddingInfoDto>()
            .ForMember(dest => dest.UserRole, opt => opt.Ignore())
            .ForMember(dest => dest.LocationCivil, opt => opt.Ignore())
            .ForMember(dest => dest.LocationChurch, opt => opt.Ignore())
            .ForMember(dest => dest.LocationParty, opt => opt.Ignore())
            .ForMember(dest => dest.LocationHouse, opt => opt.Ignore());

        CreateMap<WeddingInfoUpdateDto, WeddingInfo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }
}