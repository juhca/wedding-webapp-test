using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Application.Services;

public class GiftService : IGiftService
{
    public Task<IEnumerable<GiftDto>> GetAllVisibleAsync(Guid? currentUserId = null)
    {
        throw new NotImplementedException();
    }

    public Task<GiftDto> GetByIdAsync(Guid id, Guid? currentUserId = null)
    {
        throw new NotImplementedException();
    }

    public Task<GiftDto> CreateAsync(CreateGiftDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<GiftDto> UpdateAsync(Guid id, UpdateGiftDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<GiftReservationConfirmationDto> ReserveGiftAsync(Guid giftId, Guid userId, ReserveGiftDto dto)
    {
        throw new NotImplementedException();
    }

    public Task UnreserveGiftAsync(Guid giftId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GiftDto>> GetMyReservedGiftsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}