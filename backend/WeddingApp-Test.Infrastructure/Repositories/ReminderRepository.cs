using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class ReminderRepository(AppDbContext context) : IReminderRepository
{
    public async Task<IEnumerable<Reminder>> GetByTargetAsync(ReminderType type, Guid targetId)
    {
        return await context.Reminders
            .Where(r => r.Type == type && r.TargetId == targetId)
            .OrderBy(r => r.ScheduledFor)
            .ToListAsync();
    }

    public async Task<int> CountByTargetAsync(ReminderType type, Guid targetId)
    {
        return await context.Reminders
            .CountAsync(r => r.Type == type && r.TargetId == targetId);
    }

    public async Task<Reminder?> GetByIdAsync(Guid id)
    {
        return await context.Reminders.FindAsync(id);
    }

    public async Task AddAsync(Reminder reminder)
    {
        await context.Reminders.AddAsync(reminder);
    }

    public void Delete(Reminder reminder)
    {
        context.Reminders.Remove(reminder);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
