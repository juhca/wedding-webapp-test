using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces.Email;

public interface IEmailOutboxRepository
{
    Task AddAsync(EmailOutbox email);
    Task<EmailOutbox?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Returns Pending records where NextRetryAt is null (never tried)
    /// OR NextRetryAt is in the past (ready to retry).
    /// </summary>
    Task<IEnumerable<EmailOutbox>> GetPendingRetryableAsync(DateTime asOf);
    
    void Update(EmailOutbox email);
    Task SaveChangesAsync();
}