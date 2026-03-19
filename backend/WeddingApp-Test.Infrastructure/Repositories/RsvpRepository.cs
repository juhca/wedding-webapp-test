using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class RsvpRepository(AppDbContext context) : IRsvpRepository
{
    public async Task<Rsvp?> GetByIdAsync(Guid id)
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rsvp?> GetByUserIdAsync(Guid userId)
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<IEnumerable<Rsvp>> GetAllAsync()
    {
        return await context.Rsvps.ToListAsync();
    }

    public async Task<IEnumerable<Rsvp>> GetAllWithUsersAsync()
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .OrderBy(r => r.User.LastName)
            .ThenBy(r => r.User.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rsvp>> GetAttendingAsync()
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .Where(r => r.IsAttending)
            .OrderBy(r => r.User.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rsvp>> GetNotAttendingAsync()
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Where(r => !r.IsAttending && r.RespondedAt != null)
            .OrderBy(r => r.User.LastName)
            .ToListAsync();
    }

    public async Task<int> GetTotalGuestCountAsync()
    {
        var attending = await context.Rsvps
            .Include(r => r.Companions)
            .Where(r => r.IsAttending)
            .ToListAsync();
        
        return attending.Sum(r => r.TotalGuests);
    }

    /// <summary>
    /// Gets all attending RSVPs where either the main guest OR any companion
    /// has dietary restrictions specified.
    /// 
    /// Used for:
    /// - Catering planning
    /// - Special meal preparation
    /// - Seating arrangements
    /// 
    /// Checks two conditions:
    /// 1. Main guest has DietaryRestrictions filled in
    /// 2. OR any companion has DietaryRestrictions filled in
    /// </summary>
    /// <returns>RSVPs with dietary restrictions (main guest or companions)</returns>
    public async Task<IEnumerable<Rsvp>> GetWithDietaryRestrictionsAsync()
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .Where(r => r.IsAttending && 
                        (!string.IsNullOrEmpty(r.DietaryRestrictions) || 
                         r.Companions.Any(c => !string.IsNullOrEmpty(c.DietaryRestrictions))))
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new RSVP to the database.
    /// 
    /// NOTE: Does NOT call SaveChangesAsync - this is intentional!
    /// SaveChanges should be called in the Service layer for proper transaction control.
    /// This allows multiple repository operations to be grouped into one transaction.
    /// </summary>
    /// <param name="rsvp">The RSVP entity to add (with companions if any)</param>
    public async Task AddAsync(Rsvp rsvp)
    {
        await context.Rsvps.AddAsync(rsvp);
    }
    
    /// <summary>
    /// Marks an RSVP as modified so EF Core will update it on SaveChanges.
    /// 
    /// NOTE: Does NOT call SaveChangesAsync - this is intentional!
    /// SaveChanges should be called in the Service layer for proper transaction control.
    /// This allows multiple repository operations to be grouped into one transaction.
    /// 
    /// When updating companions:
    /// - Service layer should remove old companions explicitly
    /// - Then update the RSVP with new companions list
    /// - EF Core will handle the cascade correctly on SaveChanges
    /// </summary>
    /// <param name="rsvp">The RSVP entity to update</param> 
    public void Update(Rsvp rsvp)
    {
        context.Update(rsvp);
    }

    /// <summary>
    /// Checks if a user has already submitted an RSVP.
    /// Useful for validation before creating a new RSVP.
    /// 
    /// Note: Each user can only have ONE RSVP (enforced by unique index on UserId).
    /// </summary>
    /// <param name="userId">The user identifier to check</param>
    /// <returns>True if user has an RSVP (regardless of attendance status), false otherwise</returns>
    public async Task<bool> ExistsForUserAsync(Guid userId)
    {
        return await context.Rsvps.AnyAsync(r => r.UserId == userId);
    }

    public async Task<IEnumerable<Rsvp>> GetRespondedSinceAsync(DateTime since)
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .Where(r => r.RespondedAt >= since || r.UpdatedAt >= since)
            .OrderByDescending(r => r.RespondedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rsvp>> GetPendingWeddingRemindersAsync()
    {
        return await context.Rsvps
            .Include(r => r.User)
            .Include(r => r.Companions)
            .Where(r => r.WantsReminder && r.ReminderSentAt == null && r.IsAttending)
            .ToListAsync();
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}