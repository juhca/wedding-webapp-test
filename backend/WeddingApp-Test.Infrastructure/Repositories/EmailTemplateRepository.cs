using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class EmailTemplateRepository(AppDbContext dbContext) : IEmailTemplateRepository
{
    public async Task<IEnumerable<EmailTemplate>> GetActiveByTriggerAsync(string triggerName, Guid userId)
    {
        return await  dbContext.EmailTemplates
            .Where(e => e.TriggerName == triggerName && e.IsActive)
            .Include(t => t.SendLogs.Where(l => l.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
    {
        return await dbContext.EmailTemplates.ToListAsync();
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id)
    {
        return await dbContext.EmailTemplates.FindAsync(id);
    }

    public async Task AddAsync(EmailTemplate template)
    {
        await dbContext.EmailTemplates.AddAsync(template);
    }

    public void Update(EmailTemplate template)
    {
        dbContext.EmailTemplates.Update(template);
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await dbContext.EmailTemplates.FindAsync(id);
        if (template is not null)
        {
            dbContext.EmailTemplates.Remove(template);    
        }
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}