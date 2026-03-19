using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Email;

/// <summary>
/// Builds HTML email bodies for all email types.
/// Keep styling inline (email clients don't support external CSS).
/// </summary>
public static class EmailTemplates
{
    private const string BaseColor = "#8B5E3C";
    private const string LightBg = "#FDF8F4";
    private const string TextColor = "#3A2E28";

    // ─── Shared layout ───────────────────────────────────────────────

    private static string Wrap(string title, string body) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>{title}</title></head>
        <body style="margin:0;padding:0;background:{LightBg};font-family:Georgia,serif;color:{TextColor};">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:{LightBg};padding:32px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08);">
                <tr><td style="background:{BaseColor};padding:28px 40px;text-align:center;">
                  <h1 style="margin:0;color:#fff;font-size:22px;letter-spacing:2px;font-weight:normal;">{title}</h1>
                </td></tr>
                <tr><td style="padding:40px;">
                  {body}
                </td></tr>
                <tr><td style="background:#f5ede3;padding:20px 40px;text-align:center;font-size:12px;color:#9E8070;">
                  This email was sent automatically. Please do not reply.
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string Row(string label, string value) =>
        $"<tr><td style=\"padding:6px 0;color:#9E8070;font-size:14px;\">{label}</td>" +
        $"<td style=\"padding:6px 0;font-size:14px;\"><strong>{value}</strong></td></tr>";

    private static string Table(string rows) =>
        $"<table style=\"width:100%;border-collapse:collapse;margin-top:16px;\">{rows}</table>";

    private static string Divider() =>
        "<hr style=\"border:none;border-top:1px solid #EDE0D4;margin:24px 0;\">";

    // ─── 1. RSVP Confirmation (new) ──────────────────────────────────

    public static string RsvpConfirmation(User user, Rsvp rsvp)
    {
        var attending = rsvp.IsAttending ? "✅ Attending" : "❌ Not attending";
        var companions = rsvp.Companions.Count > 0
            ? string.Join(", ", rsvp.Companions.Select(c => $"{c.FirstName} {c.LastName}"))
            : "None";

        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>We have received your RSVP. Here is a summary:</p>
            {Divider()}
            {Table(
                Row("Status", attending) +
                Row("Companions", companions) +
                (rsvp.DietaryRestrictions is not null ? Row("Dietary notes", rsvp.DietaryRestrictions) : "") +
                (rsvp.Notes is not null ? Row("Notes", rsvp.Notes) : "")
            )}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">You can update your RSVP at any time via the wedding website.</p>
            """;

        return Wrap("RSVP Confirmed 💌", body);
    }

    // ─── 2. RSVP Updated ─────────────────────────────────────────────

    public static string RsvpUpdated(User user, Rsvp rsvp)
    {
        var attending = rsvp.IsAttending ? "✅ Attending" : "❌ Not attending";
        var companions = rsvp.Companions.Count > 0
            ? string.Join(", ", rsvp.Companions.Select(c => $"{c.FirstName} {c.LastName}"))
            : "None";

        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>Your RSVP has been updated. Here is your latest response:</p>
            {Divider()}
            {Table(
                Row("Status", attending) +
                Row("Companions", companions) +
                (rsvp.DietaryRestrictions is not null ? Row("Dietary notes", rsvp.DietaryRestrictions) : "") +
                (rsvp.Notes is not null ? Row("Notes", rsvp.Notes) : "")
            )}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">If you did not make this change, please contact us.</p>
            """;

        return Wrap("RSVP Updated 📝", body);
    }

    // ─── 3. Gift Reserved ────────────────────────────────────────────

    public static string GiftReserved(User user, Gift gift, GiftReservation reservation)
    {
        var price = gift.Price.HasValue ? $"${gift.Price:F2}" : "—";

        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>Thank you for reserving a gift! Here are the details:</p>
            {Divider()}
            {Table(
                Row("Gift", gift.Name) +
                (gift.Description is not null ? Row("Description", gift.Description) : "") +
                Row("Price", price) +
                (gift.PurchaseLink is not null ? Row("Purchase link", $"<a href=\"{gift.PurchaseLink}\" style=\"color:{BaseColor};\">{gift.PurchaseLink}</a>") : "") +
                Row("Reserved on", reservation.ReservedAt.ToString("dd MMM yyyy"))
            )}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">You can manage your gift reservations on the wedding website.</p>
            """;

        return Wrap("Gift Reserved 🎁", body);
    }

    // ─── 4. Gift Unreserved ──────────────────────────────────────────

    public static string GiftUnreserved(User user, Gift gift)
    {
        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>Your reservation for the following gift has been cancelled:</p>
            {Divider()}
            {Table(Row("Gift", gift.Name))}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">The gift is now available for others to reserve.</p>
            """;

        return Wrap("Gift Unreserved 🎁", body);
    }

    // ─── 5. Admin: Daily RSVP Digest ─────────────────────────────────

    public static string AdminRsvpDigest(IEnumerable<Rsvp> rsvps)
    {
        var list = rsvps.ToList();
        if (list.Count == 0)
        {
            return Wrap("Daily RSVP Report 📋", "<p>No new RSVP responses in the last 24 hours.</p>");
        }

        var rows = string.Join("", list.Select(r =>
            $"<tr style=\"border-bottom:1px solid #EDE0D4;\">" +
            $"<td style=\"padding:8px 4px;\">{r.User?.FirstName} {r.User?.LastName}</td>" +
            $"<td style=\"padding:8px 4px;\">{(r.IsAttending ? "✅ Attending" : "❌ Declining")}</td>" +
            $"<td style=\"padding:8px 4px;\">{r.Companions.Count} companion(s)</td>" +
            $"<td style=\"padding:8px 4px;font-size:12px;color:#9E8070;\">{r.RespondedAt?.ToString("HH:mm")}</td>" +
            "</tr>"));

        var body = $"""
            <p>Here is a summary of RSVP responses received in the last 24 hours ({list.Count} total):</p>
            <table style="width:100%;border-collapse:collapse;font-size:14px;">
              <thead><tr style="background:#f5ede3;">
                <th style="padding:8px 4px;text-align:left;">Name</th>
                <th style="padding:8px 4px;text-align:left;">Status</th>
                <th style="padding:8px 4px;text-align:left;">Companions</th>
                <th style="padding:8px 4px;text-align:left;">Time</th>
              </tr></thead>
              <tbody>{rows}</tbody>
            </table>
            """;

        return Wrap("Daily RSVP Report 📋", body);
    }

    // ─── 6. Admin: Daily Gift Digest ─────────────────────────────────

    public static string AdminGiftDigest(IEnumerable<GiftReservation> reservations)
    {
        var list = reservations.ToList();
        if (list.Count == 0)
        {
            return Wrap("Daily Gift Report 🎁", "<p>No gift reservation changes in the last 24 hours.</p>");
        }

        var rows = string.Join("", list.Select(r =>
            $"<tr style=\"border-bottom:1px solid #EDE0D4;\">" +
            $"<td style=\"padding:8px 4px;\">{r.ReservedBy?.FirstName} {r.ReservedBy?.LastName}</td>" +
            $"<td style=\"padding:8px 4px;\">{r.Gift?.Name}</td>" +
            $"<td style=\"padding:8px 4px;font-size:12px;color:#9E8070;\">{r.ReservedAt:HH:mm}</td>" +
            "</tr>"));

        var body = $"""
            <p>Gift reservation changes in the last 24 hours ({list.Count} total):</p>
            <table style="width:100%;border-collapse:collapse;font-size:14px;">
              <thead><tr style="background:#f5ede3;">
                <th style="padding:8px 4px;text-align:left;">Guest</th>
                <th style="padding:8px 4px;text-align:left;">Gift</th>
                <th style="padding:8px 4px;text-align:left;">Time</th>
              </tr></thead>
              <tbody>{rows}</tbody>
            </table>
            """;

        return Wrap("Daily Gift Report 🎁", body);
    }

    // ─── 7. Admin: Reminder Requests Digest ──────────────────────────

    public static string AdminReminderRequestsDigest(IEnumerable<User> users)
    {
        var list = users.ToList();
        if (list.Count == 0)
        {
            return Wrap("Reminder Requests Report 🔔", "<p>No new reminder requests in the last 24 hours.</p>");
        }

        var rows = string.Join("", list.Select(u =>
            $"<tr style=\"border-bottom:1px solid #EDE0D4;\">" +
            $"<td style=\"padding:8px 4px;\">{u.FirstName} {u.LastName}</td>" +
            $"<td style=\"padding:8px 4px;font-size:12px;color:#9E8070;\">{u.Email}</td>" +
            "</tr>"));

        var body = $"""
            <p>Guests who requested reminders in the last 24 hours ({list.Count} total):</p>
            <table style="width:100%;border-collapse:collapse;font-size:14px;">
              <thead><tr style="background:#f5ede3;">
                <th style="padding:8px 4px;text-align:left;">Name</th>
                <th style="padding:8px 4px;text-align:left;">Email</th>
              </tr></thead>
              <tbody>{rows}</tbody>
            </table>
            """;

        return Wrap("Reminder Requests Report 🔔", body);
    }

    // ─── 8. Guest: Wedding Day Reminder ──────────────────────────────

    public static string WeddingReminder(User user, Rsvp rsvp, DateTime weddingDate, string weddingName)
    {
        var daysLeft = (weddingDate.Date - DateTime.UtcNow.Date).Days;
        var companions = rsvp.Companions.Count > 0
            ? string.Join(", ", rsvp.Companions.Select(c => $"{c.FirstName} {c.LastName}"))
            : "None";

        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>Just a friendly reminder — <strong>{weddingName}</strong> is coming up in <strong>{daysLeft} day{(daysLeft == 1 ? "" : "s")}</strong>! 🎉</p>
            {Divider()}
            {Table(
                Row("Wedding date", weddingDate.ToString("dddd, dd MMMM yyyy")) +
                Row("Your status", rsvp.IsAttending ? "✅ Attending" : "❌ Not attending") +
                Row("Companions", companions)
            )}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">We can't wait to celebrate with you!</p>
            """;

        return Wrap($"The Big Day is Almost Here! 💍", body);
    }

    // ─── 9. Guest: Gift Purchase Reminder ────────────────────────────

    public static string GiftPurchaseReminder(User user, Gift gift, GiftReservation reservation)
    {
        var price = gift.Price.HasValue ? $"${gift.Price:F2}" : "—";

        var body = $"""
            <p style="font-size:16px;">Dear <strong>{user.FirstName}</strong>,</p>
            <p>This is a friendly reminder that you reserved the following gift and the purchase date is approaching:</p>
            {Divider()}
            {Table(
                Row("Gift", gift.Name) +
                (gift.Description is not null ? Row("Description", gift.Description) : "") +
                Row("Price", price) +
                (gift.PurchaseLink is not null ? Row("Purchase link", $"<a href=\"{gift.PurchaseLink}\" style=\"color:{BaseColor};\">{gift.PurchaseLink}</a>") : "") +
                Row("Reminder scheduled", reservation.ReminderScheduledFor?.ToString("dd MMM yyyy") ?? "—")
            )}
            {Divider()}
            <p style="font-size:14px;color:#9E8070;">Thank you for your generosity!</p>
            """;

        return Wrap("Gift Purchase Reminder 🛒", body);
    }
}
