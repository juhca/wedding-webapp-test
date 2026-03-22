using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Interfaces;

public interface IReminderRepository
{
    Task<IEnumerable<Reminder>> GetByTargetAsync(ReminderType type, Guid targetId);
    Task<int> CountByTargetAsync(ReminderType type, Guid targetId);
    Task<Reminder?> GetByIdAsync(Guid id);
    Task AddAsync(Reminder reminder);
    void Delete(Reminder reminder);
    Task SaveChangesAsync();
}
