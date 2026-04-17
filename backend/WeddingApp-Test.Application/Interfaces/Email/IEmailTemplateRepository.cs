using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces.Email;

public interface IEmailTemplateRepository
{
    /// <summary>
    /// Returns active templates matching the trigger name, with send logs for the given user loaded.
    /// </summary>
    Task<IEnumerable<EmailTemplate>> GetActiveByTriggerAsync(string triggerName, Guid userId);

    Task<IEnumerable<EmailTemplate>> GetAllAsync();
    Task<EmailTemplate?> GetByIdAsync(Guid id);
    Task AddAsync(EmailTemplate template);
    void Update(EmailTemplate template);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}