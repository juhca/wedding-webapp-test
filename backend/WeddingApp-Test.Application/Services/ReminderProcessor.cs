using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class ReminderProcessor(IReminderRepository reminderRepository, IRsvpRepository rsvpRepository, IGiftRepository giftRepository, IEmailService emailService, ILogger<ReminderProcessor> logger) : IReminderProcessor
{
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var yesterday = today.AddDays(-1);

        logger.LogInformation("Processing reminders for {Date:yyyy-MM-dd}.", today);

        var pending = (await reminderRepository.GetPendingRemindersAsync(now)).ToList();

        if (pending.Count == 0)
        {
            logger.LogInformation("No pending reminders found.");
            return;
        }

        var normal = pending.Where(r => r.ScheduledFor.Date >= yesterday).ToList();
        var missed = pending.Where(r => r.ScheduledFor.Date < yesterday).ToList();

        // --- Normal reminders (today or yesterday) ---
        foreach (var reminder in normal)
        {
            var recipientEmail = await ResolveRecipientEmailAsync(reminder, cancellationToken);
            if (recipientEmail is null)
            {
                logger.LogWarning(
                    "Could not resolve recipient for reminder {ReminderId} (Type={Type}, TargetId={TargetId}) — marking as handled to avoid endless retries.",
                    reminder.Id, reminder.Type, reminder.TargetId);

                reminder.SentAt = now;
                continue;
            }

            try
            {
                await emailService.SendReminderEmailAsync(recipientEmail, reminder, cancellationToken);
                reminder.SentAt = now;
            }
            catch (EmailDeliveryException ex) when (ex.IsTransient)
            {
                logger.LogError(ex, "All email providers are down — aborting batch. Pending reminders will retry on the next run.");
                break;
            }
            catch (EmailDeliveryException ex)
            {
                logger.LogError(ex, "Permanent delivery failure for reminder {ReminderId} — skipping.", reminder.Id);
            }
        }

        if (normal.Count > 0)
        {
            logger.LogInformation("Sent {Count} reminder(s).", normal.Count);
        }

        // --- Missed reminders (older than yesterday) ---
        foreach (var reminder in missed)
        {
            // TODO: implement special handling (e.g. send a late notification, alert admin, etc.)
            logger.LogWarning(
                "Missed reminder {ReminderId} was scheduled for {ScheduledFor:yyyy-MM-dd} but was never sent. Special handling required.",
                reminder.Id, reminder.ScheduledFor);
        }

        if (missed.Count > 0)
        {
            logger.LogWarning("{Count} missed reminder(s) require special handling.", missed.Count);
        }

        await reminderRepository.SaveChangesAsync();
    }

    private async Task<string?> ResolveRecipientEmailAsync(Reminder reminder, CancellationToken ct)
    {
        return reminder.Type switch
        {
            ReminderType.Rsvp =>
                (await rsvpRepository.GetByIdAsync(reminder.TargetId))?.User.Email,

            ReminderType.Gift =>
                (await giftRepository.GetReservationAsync(reminder.TargetId))?.ReservedBy.Email,

            _ => null
        };
    }
}
