using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.BackgroundServices;

/// <summary>
/// Runs once per day at the configured hour (EmailConfig:DailyReportHour).
/// Sends three admin digest emails:
///   1. RSVP responses from the past 24h
///   2. Gift reservation changes from the past 24h
///   3. Guests who requested RSVP or gift reminders in the past 24h
/// </summary>
public class DailyAdminReportService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<DailyAdminReportService> _logger;

    public DailyAdminReportService(
        IServiceProvider services,
        IConfiguration config,
        ILogger<DailyAdminReportService> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextRun();
            _logger.LogInformation("DailyAdminReportService sleeping {Hours:F1}h until next run", delay.TotalHours);
            await Task.Delay(delay, stoppingToken);

            try
            {
                await RunReportsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "DailyAdminReportService failed during report run");
            }
        }
    }

    private async Task RunReportsAsync(CancellationToken ct)
    {
        _logger.LogInformation("DailyAdminReportService: running daily reports");

        await using var scope = _services.CreateAsyncScope();
        var rsvpRepo = scope.ServiceProvider.GetRequiredService<IRsvpRepository>();
        var giftRepo = scope.ServiceProvider.GetRequiredService<IGiftRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var since = DateTime.UtcNow.AddHours(-24);

        // 1. RSVP digest
        var recentRsvps = (await rsvpRepo.GetRespondedSinceAsync(since)).ToList();
        await emailService.SendAdminRsvpDigestAsync(recentRsvps, ct);
        _logger.LogInformation("Sent RSVP digest ({Count} responses)", recentRsvps.Count);

        // 2. Gift reservations digest
        var recentReservations = (await giftRepo.GetReservationsSinceAsync(since)).ToList();
        await emailService.SendAdminGiftDigestAsync(recentReservations, ct);
        _logger.LogInformation("Sent gift digest ({Count} reservations)", recentReservations.Count);

        // 3. Reminder requests digest
        // Guests who set WantsReminder=true on RSVP OR ReminderRequested=true on a gift reservation, both in last 24h
        var rsvpReminders = await rsvpRepo.GetRespondedSinceAsync(since);
        var usersWithRsvpReminder = rsvpReminders
            .Where(r => r.WantsReminder)
            .Select(r => r.User)
            .Where(u => u is not null)
            .ToList();

        var giftReminders = await giftRepo.GetReservationsSinceAsync(since);
        var usersWithGiftReminder = giftReminders
            .Where(r => r.ReminderRequested)
            .Select(r => r.ReservedBy)
            .Where(u => u is not null)
            .ToList();

        var usersWhoRequestedReminders = usersWithRsvpReminder
            .Concat(usersWithGiftReminder)
            .DistinctBy(u => u!.Id)
            .Select(u => u!)
            .ToList();

        await emailService.SendAdminReminderRequestsDigestAsync(usersWhoRequestedReminders, ct);
        _logger.LogInformation("Sent reminder requests digest ({Count} users)", usersWhoRequestedReminders.Count);
    }

    private TimeSpan TimeUntilNextRun()
    {
        var reportHour = _config.GetValue<int>("EmailConfig:DailyReportHour", 8);
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(reportHour);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}
