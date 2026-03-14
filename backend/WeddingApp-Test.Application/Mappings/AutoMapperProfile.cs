using AutoMapper;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.DTO.Rsvp;
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
        
        // Rsvp Mappings
        CreateMap<Rsvp, RsvpDto>()
            .ForMember(dest => dest.MaxCompanionsAllowed, opt => opt.Ignore())
            .ForMember(dest => dest.TotalGuests, opt => opt.Ignore());

        CreateMap<Rsvp, RsvpWithUserDto>()
            .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.MaxCompanionsAllowed, opt => opt.MapFrom(src => src.User.MaxCompanions ?? 0))
            .ForMember(dest => dest.TotalGuests, opt => opt.Ignore()); 
        
        CreateMap<CreateRsvpDto, Rsvp>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Companions, opt => opt.Ignore())
            .ForMember(dest => dest.RespondedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReminderSentAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalGuests, opt => opt.Ignore());
        
        // GuestCompanion Mappings
        CreateMap<GuestCompanion, GuestCompanionDto>();
        
        CreateMap<CreateGuestCompanionDto, GuestCompanion>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RsvpId, opt => opt.Ignore())
            .ForMember(dest => dest.Rsvp, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        // GIFT MAPPINGS
        CreateMap<Gift, GiftDto>()
            .ForMember(dest => dest.IsReservedByMe, opt => opt.Ignore())
            .ForMember(dest => dest.ReservationStatus, opt => opt.Ignore());
        
        
        CreateMap<CreateGiftDto, Gift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Reservations, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReservationCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsFullyReserved, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingReservations, opt => opt.Ignore());

        CreateMap<UpdateGiftDto, Gift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Reservations, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReservationCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsFullyReserved, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingReservations, opt => opt.Ignore());

        // GIFT RESERVATION MAPPINGS
        CreateMap<GiftReservation, GiftReservationDto>()
            .ForMember(dest => dest.ReservedByName, opt => opt.MapFrom(src =>
                $"{src.ReservedBy.FirstName} {src.ReservedBy.LastName}"));
    }
}