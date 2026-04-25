using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces.Email;

/// <summary>
/// Dispatches transactional emails in response to domain events.
/// Resolves matching templates, renders them, writes an outbox record,
/// logs the dispatch for dedup tracking, and notifies the background processor.
/// </summary>
public interface IEmailDispatchService
{
    /// <summary>
    /// Processes a domain event by finding all active email templates registered
    /// for <paramref name="eventName"/>, rendering them against the supplied
    /// <paramref name="context"/>, and queuing the resulting messages for delivery.
    /// </summary>
    /// <param name="eventName">
    /// The trigger key used to look up matching templates (e.g. <c>"rsvp.confirmed"</c>).
    /// </param>
    /// <param name="triggeredBy">
    /// The user who caused the event and will receive the email.
    /// Dispatch is skipped silently when this user has no email address.
    /// </param>
    /// <param name="context">
    /// Caller-supplied Liquid render variables merged with <c>User</c> and <c>Wedding</c>
    /// before rendering. May include <c>RelatedEntityId</c> (<see cref="Guid"/>) to
    /// associate the outbox record with a related domain entity.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task DispatchEventAsync(string eventName, User triggeredBy, Dictionary<string, object?> context, CancellationToken ct);
}
