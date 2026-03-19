using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Email;

/// <summary>
/// High-level email service. Composes HTML via EmailTemplates and delegates to IEmailProvider.
/// Failures are caught and logged so they never crash the calling service.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailProvider _provider;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailProvider provider, IConfiguration config, ILogger<EmailService> logger)
    {
        _provider = provider;
        _config = config;
        _logger = logger;
    }

    private string AdminEmail => _config["EmailConfig:AdminEmail"]!;
    private string WeddingName => _config["EmailConfig:FromName"] ?? "The Wedding";

    // ─── Guest transactional ───────────────────────────────────────

    public async Task SendRsvpConfirmationAsync(User user, Rsvp rsvp, CancellationToken ct = default)
    {
        await SendSafe(
            user.Email,
            "Your RSVP is confirmed 💌",
            EmailTemplates.RsvpConfirmation(user, rsvp),
            ct);
    }

    public async Task SendRsvpUpdatedAsync(User user, Rsvp rsvp, CancellationToken ct = default)
    {
        await SendSafe(
            user.Email,
            "Your RSVP has been updated 📝",
            EmailTemplates.RsvpUpdated(user, rsvp),
            ct);
    }

    public async Task SendGiftReservedAsync(User user, Gift gift, GiftReservation reservation, CancellationToken ct = default)
    {
        await SendSafe(
            user.Email,
            $"Gift reserved: {gift.Name} 🎁",
            EmailTemplates.GiftReserved(user, gift, reservation),
            ct);
    }

    public async Task SendGiftUnreservedAsync(User user, Gift gift, CancellationToken ct = default)
    {
        await SendSafe(
            user.Email,
            $"Gift reservation cancelled: {gift.Name}",
            EmailTemplates.GiftUnreserved(user, gift),
            ct);
    }

    // ─── Admin daily digests ───────────────────────────────────────

    public async Task SendAdminRsvpDigestAsync(IEnumerable<Rsvp> rsvpsInLast24H, CancellationToken ct = default)
    {
        await SendSafe(
            AdminEmail,
            $"[Daily] RSVP Report — {DateTime.UtcNow:dd MMM yyyy}",
            EmailTemplates.AdminRsvpDigest(rsvpsInLast24H),
            ct);
    }

    public async Task SendAdminGiftDigestAsync(IEnumerable<GiftReservation> reservationsInLast24H, CancellationToken ct = default)
    {
        await SendSafe(
            AdminEmail,
            $"[Daily] Gift Report — {DateTime.UtcNow:dd MMM yyyy}",
            EmailTemplates.AdminGiftDigest(reservationsInLast24H),
            ct);
    }

    public async Task SendAdminReminderRequestsDigestAsync(IEnumerable<User> usersWhoRequestedReminders, CancellationToken ct = default)
    {
        await SendSafe(
            AdminEmail,
            $"[Daily] Reminder Requests — {DateTime.UtcNow:dd MMM yyyy}",
            EmailTemplates.AdminReminderRequestsDigest(usersWhoRequestedReminders),
            ct);
    }

    // ─── Guest scheduled reminders ────────────────────────────────

    public async Task SendWeddingReminderAsync(User user, Rsvp rsvp, CancellationToken ct = default)
    {
        var weddingDate = DateTime.Parse(_config["EmailConfig:WeddingDate"]!);
        await SendSafe(
            user.Email,
            $"The big day is almost here! 💍",
            EmailTemplates.WeddingReminder(user, rsvp, weddingDate, WeddingName),
            ct);
    }

    public async Task SendGiftPurchaseReminderAsync(User user, Gift gift, GiftReservation reservation, CancellationToken ct = default)
    {
        await SendSafe(
            user.Email,
            $"Reminder: purchase {gift.Name} 🛒",
            EmailTemplates.GiftPurchaseReminder(user, gift, reservation),
            ct);
    }

    // ─── Helper ───────────────────────────────────────────────────

    private async Task SendSafe(string to, string subject, string html, CancellationToken ct)
    {
        try
        {
            await _provider.SendAsync(to, subject, html, ct);
        }
        catch (Exception ex)
        {
            // Emails are best-effort — log the failure but don't crash the caller
            _logger.LogError(ex, "Failed to send email '{Subject}' to {To}", subject, to);
        }
    }
}
