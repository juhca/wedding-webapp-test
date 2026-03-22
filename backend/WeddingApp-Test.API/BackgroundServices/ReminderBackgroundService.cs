using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.BackgroundServices;

// When the app shuts down, the CancellationToken (stoppingToken) is cancelled, which causes
// WaitForNextTickAsync to return false and the loop to exit cleanly.
public class ReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReminderBackgroundService started.");

        // PeriodicTimer ticks immediately on the first WaitForNextTickAsync call (inside the do-while),
        // so reminders are processed once on startup and then every 24 hours after that.
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                // BackgroundService is a singleton, but IReminderProcessor depends on scoped services
                // (repositories, DbContext). We create a fresh scope per tick so they are properly
                // instantiated and disposed after each run.
                await using var scope = scopeFactory.CreateAsyncScope();
                // extracted processing from ReminderBackgroundService, so it can be tested
                var processor = scope.ServiceProvider.GetRequiredService<IReminderProcessor>();
                await processor.ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error while processing reminders.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
