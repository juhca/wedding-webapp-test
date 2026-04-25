using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.Infrastructure.Email;

public class EmailOutboxProcessor(IEmailEventChannel channel, IServiceScopeFactory scopeFactory, ILogger<EmailOutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Run both loops concurrently
        await Task.WhenAll(RunChannelLoopAsync(ct), RunSweepLoopAsync(ct));
    }
    
    // Loop 1: process outbox IDs as they arrive from the channel
    private async Task RunChannelLoopAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var outboxId in channel.ReadAllAsync(ct))
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<EmailOutboxProcessorService>();

                try
                {
                    await processor.ProcessByIdAsync(outboxId, ct);
                }
                catch (Exception ex)
                {
                   logger.LogError(ex, "Unhandled error processing outbox {Id}", outboxId); 
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — channel.ReadAllAsync throws OperationCanceledException when ct fires
        }
}
    
    // Loop 2: every 15min, check for missed/retry-due records
    private async Task RunSweepLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<EmailOutboxProcessorService>();
                await processor.ProcessPendingRetriesAsync(ct);
                await Task.Delay(TimeSpan.FromMinutes(15), ct);
            }
            catch (OperationCanceledException)
            {
                break; // Shutdown requested
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during outbox sweep");
            }
        }
    }
}