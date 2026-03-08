using AutoMapper;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Services;

public class GiftService(IGiftRepository giftRepository, IUserRepository userRepository, IMapper mapper) : IGiftService
{
    public async Task<IEnumerable<GiftDto>> GetAllVisibleAsync(Guid? currentUserId = null)
    {
        var gifts = await giftRepository.GetVisibleAsync();
        var dtos = mapper.Map<IEnumerable<GiftDto>>(gifts).ToList();

        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos)
            {
                var gift = gifts.First(g => g.Id == dto.Id);
                dto.IsReservedByMe = gift.Reservations.Any(r => r.ReservedByUserId == currentUserId.Value);
            }
        }

        return dtos;
    }

    public async Task<GiftDto> GetByIdAsync(Guid id, Guid? currentUserId = null)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(id);

        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }
        
        var dto = mapper.Map<GiftDto>(gift);
        
        if (currentUserId.HasValue)
        {
            dto.IsReservedByMe = gift.Reservations.Any(r => r.ReservedByUserId == currentUserId.Value);
        }
        
        return dto;
    }

    public async Task<GiftDto> CreateAsync(CreateGiftDto dto)
    {
        var gift = mapper.Map<Gift>(dto);
        gift.Id = Guid.NewGuid();
        gift.CreatedAt = DateTime.UtcNow;
        
        await giftRepository.AddAsync(gift);
        await giftRepository.SaveChangesAsync();
        
        return mapper.Map<GiftDto>(gift);
    }

    public async Task<GiftDto> UpdateAsync(Guid id, UpdateGiftDto dto)
    {
        var gift = await giftRepository.GetByIdAsync(id);
        
        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }
        
        mapper.Map(dto, gift);
        gift.UpdatedAt = DateTime.UtcNow;
        
        giftRepository.Update(gift);
        await giftRepository.SaveChangesAsync();
        
        return mapper.Map<GiftDto>(gift);
    }

    public async Task DeleteAsync(Guid id)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(id);

        if (gift == null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }

        // TODO(MAYBE DELETE THIS IN THE FUTURE ~ or make it optional)
        if (gift.Reservations.Any())
        {
            throw new InvalidOperationException("Cannot delete a gift that has reservations");
        }

        giftRepository.Delete(gift);
        await giftRepository.SaveChangesAsync();
    }

    public async Task<GiftReservationConfirmationDto> ReserveGiftAsync(Guid giftId, Guid userId, ReserveGiftDto dto)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(giftId);

        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {giftId} not found");
        }
        
        // Check 1: is gift fully reserved?
        if (gift.IsFullyReserved)
        {
            throw new InvalidOperationException("This gift is fully reserved");
        }
        
        // Check 2: has user already reserved this gift?
        var existingReservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId);
        if (existingReservation is not null)
        {
            throw new InvalidOperationException("You have already reserved this gift");
        }
        
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }

        var reservation = new GiftReservation
        {
            Id = Guid.NewGuid(),
            GiftId = giftId,   
            ReservedByUserId = userId,
            ReservedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            ReminderRequested = dto.WantsReminder,
            ReminderScheduledFor = dto.ReminderDate,
        };
        
        await giftRepository.AddReservationAsync(reservation);
        gift.UpdatedAt = DateTime.UtcNow;
        giftRepository.Update(gift);
        await giftRepository.SaveChangesAsync();
        
        // Reload to get updated counts
        gift = await giftRepository.GetByIdWithReservationsAsync(giftId);
        
        // TODO(TOMAS): send confirmation email
        // ex.: await _emailService.SendGiftReservationConfirmationAsync(...)

        return new GiftReservationConfirmationDto
        {
            ReservationId = reservation.Id,
            GiftId = gift!.Id,
            GiftName = gift.Name,
            PurchaseLink = gift.PurchaseLink,
            Message = "Gift reserved successfully! Check your email for details.",
            ReminderScheduled = dto.WantsReminder,
            ReminderDate = dto.ReminderDate,
            RemainingReservations = gift.RemainingReservations ?? 0,
            GiftFullyReserved = gift.IsFullyReserved
        };
    }

    public async Task UnreserveGiftAsync(Guid giftId, Guid userId)
    {
        var reservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId);
        if (reservation is null)
        {
            throw new KeyNotFoundException($"Reservation not found");
        }
        
        giftRepository.DeleteReservation(reservation);
        var gift =  await giftRepository.GetByIdAsync(giftId);
        if (gift is not null)
        {
            gift.UpdatedAt = DateTime.UtcNow;
            giftRepository.Update(gift);
        }
        
        await giftRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<GiftDto>> GetMyReservedGiftsAsync(Guid userId)
    {
        var gifts = await giftRepository.GetReservedByUserAsync(userId);
        var dtos = mapper.Map<IEnumerable<GiftDto>>(gifts).ToList();

        foreach (var dto in dtos)
        {
            dto.IsReservedByMe = true;
        }

        return dtos;
    }
}