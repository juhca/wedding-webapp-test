using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces.Email;

public interface IEmailSendLogRepository
{
    Task AddAsync(EmailSendLog log);

    /// <summary>
    /// Count of dispatched (queued) emails for this template/user pair.
    /// Used for MaxSendsPerUser dedup. "Dispatched" = outbox record was created, not necessarily delivered.
    /// </summary>
    Task<int> CountDispatchedAsync(Guid templateId, Guid userId);

    Task SaveChangesAsync();
}