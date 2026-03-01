using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IRsvpRepository
{
    Task<Rsvp?> GetByIdAsync(Guid id);
    Task<Rsvp?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Rsvp>> GetAllAsync();
    Task<IEnumerable<Rsvp>> GetAllWithUsersAsync();
    Task<IEnumerable<Rsvp>> GetAttendingAsync();
    Task<IEnumerable<Rsvp>> GetNotAttendingAsync();
    Task<int> GetTotalGuestCountAsync();
    Task<IEnumerable<Rsvp>> GetWithDietaryRestrictionsAsync();
    Task AddAsync(Rsvp rsvp);
    void Update(Rsvp rsvp);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task<int> SaveChangesAsync();
}