using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class EmailSendLogRepository(AppDbContext dbContext) : IEmailSendLogRepository
{
    public async Task AddAsync(EmailSendLog log)
    {
        await  dbContext.EmailSendLogs.AddAsync(log);
    }

    public async Task<int> CountDispatchedAsync(Guid templateId, Guid userId)
    {
        return await  dbContext.EmailSendLogs
            .Where(e => e.TemplateId == templateId && e.UserId == userId && e.Dispatched)
            .CountAsync();
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}