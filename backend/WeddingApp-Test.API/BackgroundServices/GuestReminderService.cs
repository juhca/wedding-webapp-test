using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.BackgroundServices;

/// <summary>
/// Runs once per day and sends scheduled reminder emails to guests:
///   1. Wedding-day reminder — guests with WantsReminder=true whose reminder hasn't been sent yet,
///      when the wedding is within the configured ReminderDaysBefore window.
///   2. Gift purchase reminder — gift reservations where ReminderScheduledFor has arrived
///      and the reminder hasn't been sent yet.
/// After sending, marks ReminderSentAt so reminders are only sent once.
/// </summary>
public class GuestReminderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<GuestReminderService> _logger;

    public GuestReminderService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<GuestReminderService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once at startup after a short delay, then daily
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "GuestReminderService failed during reminder run");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task RunRemindersAsync(CancellationToken ct)
    {
        _logger.LogInformation("GuestReminderService: checking pending reminders");

        await using var scope = _services.CreateAsyncScope();
        var rsvpRepo = scope.ServiceProvider.GetRequiredService<IRsvpRepository>();
        var giftRepo = scope.ServiceProvider.GetRequiredService<IGiftRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;

        // 1. Wedding-day reminders
        var weddingDateStr = _config["EmailConfig:WeddingDate"];
        if (DateTime.TryParse(weddingDateStr, out var weddingDate))
        {
            var reminderDaysBefore = _config.GetValue<int>("EmailConfig:ReminderDaysBefore", 7);
            var daysUntilWedding = (weddingDate.Date - today).Days;

            if (daysUntilWedding <= reminderDaysBefore && daysUntilWedding >= 0)
            {
                var pending = (await rsvpRepo.GetPendingWeddingRemindersAsync()).ToList();
                _logger.LogInformation("Sending wedding reminders to {Count} guests", pending.Count);

                foreach (var rsvp in pending)
                {
                    await emailService.SendWeddingReminderAsync(rsvp.User, rsvp, ct);

                    rsvp.ReminderSentAt = DateTime.UtcNow;
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(ct);
            }
        }
        else
        {
            _logger.LogWarning("WeddingDate not configured or invalid — skipping wedding reminders");
        }

        // 2. Gift purchase reminders
        var pendingGiftReminders = (await giftRepo.GetPendingGiftRemindersAsync(DateTime.UtcNow)).ToList();
        _logger.LogInformation("Sending gift purchase reminders to {Count} guests", pendingGiftReminders.Count);

        foreach (var reservation in pendingGiftReminders)
        {
            await emailService.SendGiftPurchaseReminderAsync(reservation.ReservedBy, reservation.Gift, reservation, ct);

            reservation.ReminderSentAt = DateTime.UtcNow;
        }

        if (pendingGiftReminders.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
