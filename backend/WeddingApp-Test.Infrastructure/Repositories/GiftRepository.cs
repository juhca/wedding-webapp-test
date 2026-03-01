using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class GiftRepository : IGiftRepository
{
    public Task<Gift?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Gift?> GetByIdWithReservationsAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gift>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gift>> GetVisibleAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gift>> GetAvailableAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gift>> GetFullyReservedAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Gift>> GetReservedByUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Gift gift)
    {
        throw new NotImplementedException();
    }

    public void Update(Gift gift)
    {
        throw new NotImplementedException();
    }

    public void Delete(Gift gift)
    {
        throw new NotImplementedException();
    }

    public Task<GiftReservation?> GetReservationAsync(Guid reservationId)
    {
        throw new NotImplementedException();
    }

    public Task<GiftReservation?> GetUserReservationForGiftAsync(Guid giftId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task AddReservationAsync(GiftReservation reservation)
    {
        throw new NotImplementedException();
    }

    public void DeleteReservation(GiftReservation reservation)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasUserReservedGiftAsync(Guid giftId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetReservationCountAsync(Guid giftId)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}