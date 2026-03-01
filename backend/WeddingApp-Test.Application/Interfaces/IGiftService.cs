using WeddingApp_Test.Application.DTO.Gift;

namespace WeddingApp_Test.Application.Interfaces;

public interface IGiftService
{
    Task<IEnumerable<GiftDto>> GetAllVisibleAsync(Guid? currentUserId = null);
    Task<GiftDto> GetByIdAsync(Guid id, Guid? currentUserId = null);
    Task<GiftDto> CreateAsync(CreateGiftDto dto);
    Task<GiftDto> UpdateAsync(Guid id, UpdateGiftDto dto);
    Task DeleteAsync(Guid id);
    Task<GiftReservationConfirmationDto> ReserveGiftAsync(Guid giftId, Guid userId, ReserveGiftDto dto);
    Task UnreserveGiftAsync(Guid giftId, Guid userId);
    Task<IEnumerable<GiftDto>> GetMyReservedGiftsAsync(Guid userId);
}