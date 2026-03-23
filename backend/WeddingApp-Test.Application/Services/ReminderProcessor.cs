using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Application.Services;

public class ReminderProcessor(IReminderRepository reminderRepository, IEmailService emailService, ILogger<ReminderProcessor> logger) : IReminderProcessor
{
    // TODO: replace with real recipient email resolved from the reminder's target (User)
    private const string TestRecipientEmail = "test@example.com";

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
            await emailService.SendReminderEmailAsync(TestRecipientEmail, reminder, cancellationToken);
            reminder.SentAt = now;
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
}
