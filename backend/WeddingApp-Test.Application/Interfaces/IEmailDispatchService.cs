using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IEmailDispatchService
{
    /// <summary>
    /// Dispatches a named event, triggering any matching active EmailTemplate records.
    /// Best-effort: errors are logged but not thrown, to avoid disrupting the triggering operation.
    /// </summary>
    Task DispatchEventAsync(string eventName, User triggeredBy, Dictionary<string, object?> context, CancellationToken ct = default);
}
