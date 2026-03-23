namespace WeddingApp_Test.Application.Email;

/// <summary>
/// Thrown by IEmailService when all configured providers have been exhausted
/// and the email could not be delivered.
/// IsTransient = true  → infrastructure is down (5xx/network); abort the batch, retry next tick.
/// IsTransient = false → permanent failure (bad address, invalid key); skip this reminder only.
/// </summary>
public class EmailDeliveryException(Guid reminderId, bool isTransient)
    : Exception($"All email providers failed for reminder {reminderId}. Email was not delivered.")
{
    public bool IsTransient { get; } = isTransient;
}
