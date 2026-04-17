using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class EmailOutboxRepository(AppDbContext dbContext) : IEmailOutboxRepository 
{
    public async Task AddAsync(EmailOutbox email)
    {
        await dbContext.AddAsync(email);
    }

    public async Task<EmailOutbox?> GetByIdAsync(Guid id)
    {
        return await dbContext.FindAsync<EmailOutbox>(id);
    }

    public async Task<IEnumerable<EmailOutbox>> GetPendingRetryableAsync(DateTime asOf)
    {
        return await dbContext.EmailOutbox
            .Where(e => e.Status == EmailStatus.Pending 
                        && (e.NextRetryAt == null || e.NextRetryAt <= asOf))
            .ToListAsync();
    }

    public void Update(EmailOutbox email)
    {
        dbContext.Update(email);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}