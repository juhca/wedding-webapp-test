using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class GiftRepository(AppDbContext context) : IGiftRepository
{
    public async Task<Gift?> GetByIdAsync(Guid id)
    {
        return await context.Gifts.FindAsync(id);
    }

    public async Task<Gift?> GetByIdWithReservationsAsync(Guid id)
    {
        return await context.Gifts
            .Include(g => g.Reservations)
                .ThenInclude(r => r.ReservedBy)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Gift>> GetAllAsync()
    {
        return await context.Gifts
            .Include(g => g.Reservations)
                .ThenInclude(r => r.ReservedBy)
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gift>> GetVisibleAsync()
    {
        return await context.Gifts
            .Include(g => g.Reservations)
                .ThenInclude(r => r.ReservedBy)
            .Where(g => g.IsVisible)
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gift>> GetAvailableAsync()
    {
        return await context.Gifts
            .Include(g => g.Reservations)
                .ThenInclude(r => r.ReservedBy)
            .Where(g => g.IsVisible && (!g.MaxReservations.HasValue || // Unlimited
                                        g.Reservations.Count < g.MaxReservations.Value)) // Has slots
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gift>> GetFullyReservedAsync()
    {
        return await context.Gifts
            .Include(g => g.Reservations)
                .ThenInclude(r => r.ReservedBy)
            .Where(g => g.IsFullyReserved)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gift>> GetReservedByUserAsync(Guid userId)
    {
        return await context.Gifts
            .Include(g => g.Reservations)
            .Where(g => g.Reservations.Any(r => r.ReservedByUserId == userId))
            .ToListAsync();
    }

    public async Task AddAsync(Gift gift)
    {
        await context.Gifts.AddAsync(gift);
    }

    public void Update(Gift gift)
    {
        context.Gifts.Update(gift);
    }

    public void Delete(Gift gift)
    {
        context.Gifts.Remove(gift);
    }

    // Reservation methods
    public async Task<GiftReservation?> GetReservationAsync(Guid reservationId)
    {
        return await context.GiftReservations
            .Include(r => r.Gift)
            .Include(r => r.ReservedBy)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<GiftReservation?> GetUserReservationForGiftAsync(Guid giftId, Guid userId)
    {
        return await context.GiftReservations
            .FirstOrDefaultAsync(r => r.GiftId == giftId && r.ReservedByUserId == userId);
    }

    public async Task AddReservationAsync(GiftReservation reservation)
    {
        await context.GiftReservations.AddAsync(reservation);
    }

    public async void DeleteReservation(GiftReservation reservation)
    {
        context.GiftReservations.Remove(reservation);
    }

    public async Task<bool> HasUserReservedGiftAsync(Guid giftId, Guid userId)
    {
        return await  context.GiftReservations
            .AnyAsync(r => r.GiftId == giftId && r.ReservedByUserId == userId);
    }

    public async Task<int> GetReservationCountAsync(Guid giftId)
    {
        return await  context.GiftReservations
            .CountAsync(r => r.GiftId == giftId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}