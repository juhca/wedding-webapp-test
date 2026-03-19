using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IGiftRepository
{
    Task<Gift?> GetByIdAsync(Guid id);
    Task<Gift?> GetByIdWithReservationsAsync(Guid id);
    Task<IEnumerable<Gift>> GetAllAsync();
    Task<IEnumerable<Gift>> GetVisibleAsync();
    Task<IEnumerable<Gift>> GetAvailableAsync(); // Has remaining reservations
    Task<IEnumerable<Gift>> GetFullyReservedAsync();
    Task<IEnumerable<Gift>> GetReservedByUserAsync(Guid userId);
    Task AddAsync(Gift gift);
    void Update(Gift gift);
    void Delete(Gift gift);
    
    // GiftReservation operations
    Task<GiftReservation?> GetReservationAsync(Guid reservationId);
    Task<GiftReservation?> GetUserReservationForGiftAsync(Guid giftId, Guid userId);
    Task<IEnumerable<GiftReservation>> GetReservationsSinceAsync(DateTime since);
    Task<IEnumerable<GiftReservation>> GetPendingGiftRemindersAsync(DateTime asOf);
    Task AddReservationAsync(GiftReservation reservation);
    void DeleteReservation(GiftReservation reservation);
    Task<bool> HasUserReservedGiftAsync(Guid giftId, Guid userId);
    Task<int> GetReservationCountAsync(Guid giftId);
    Task<int> SaveChangesAsync();
}