using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

/// <summary>
/// High-level email service. Composes templates and delegates sending to IEmailProvider.
/// </summary>
public interface IEmailService
{
    // --- Guest transactional emails ---
    Task SendRsvpConfirmationAsync(User user, Rsvp rsvp, CancellationToken ct = default);
    Task SendRsvpUpdatedAsync(User user, Rsvp rsvp, CancellationToken ct = default);
    Task SendGiftReservedAsync(User user, Gift gift, GiftReservation reservation, CancellationToken ct = default);
    Task SendGiftUnreservedAsync(User user, Gift gift, CancellationToken ct = default);

    // --- Admin daily digests ---
    Task SendAdminRsvpDigestAsync(IEnumerable<Rsvp> rsvpsInLast24H, CancellationToken ct = default);
    Task SendAdminGiftDigestAsync(IEnumerable<GiftReservation> reservationsInLast24H, CancellationToken ct = default);
    Task SendAdminReminderRequestsDigestAsync(IEnumerable<User> usersWhoRequestedReminders, CancellationToken ct = default);

    // --- Scheduled guest reminders ---
    Task SendWeddingReminderAsync(User user, Rsvp rsvp, CancellationToken ct = default);
    Task SendGiftPurchaseReminderAsync(User user, Gift gift, GiftReservation reservation, CancellationToken ct = default);
}
