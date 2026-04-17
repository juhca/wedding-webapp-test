namespace WeddingApp_Test.Application.Interfaces.Email;

// The in-process channel. Services publish an outbox ID. The background processor reads it.
public interface IEmailEventChannel
{
    /// <summary>
    /// Push an outbox record ID onto the channel. Non-blocking.
    /// </summary>
    ValueTask PublishAsync(Guid outboxId, CancellationToken ct = default);

    /// <summary>
    /// Read IDs from the channel one by one. Blocks when empty.
    /// </summary>
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct);
}
