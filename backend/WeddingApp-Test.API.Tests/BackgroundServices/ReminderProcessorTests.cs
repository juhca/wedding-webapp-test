using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.BackgroundServices;

[Trait("Category", "ReminderProcessor Tests")]
[Collection("Sequential")]
public class ReminderProcessorTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{

    [Fact]
    public async Task ProcessAsync_TodayReminder_SetsSentAt()
    {
        await factory.ResetDatabaseAsync();
        var reminder = MakeReminder(scheduledFor: DateTime.UtcNow.Date);
        await SeedReminder(reminder);

        await RunProcessor();

        var result = await GetReminder(reminder.Id);
        Assert.NotNull(result!.SentAt);
    }

    [Fact]
    public async Task ProcessAsync_YesterdayReminder_SetsSentAt()
    {
        await factory.ResetDatabaseAsync();
        var reminder = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(-1));
        await SeedReminder(reminder);

        await RunProcessor();

        var result = await GetReminder(reminder.Id);
        Assert.NotNull(result!.SentAt);
    }

    [Fact]
    public async Task ProcessAsync_MissedReminder_DoesNotSetSentAt()
    {
        await factory.ResetDatabaseAsync();
        // Older than yesterday → missed, should not be marked as sent
        var reminder = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(-5));
        await SeedReminder(reminder);

        await RunProcessor();

        var result = await GetReminder(reminder.Id);
        Assert.Null(result!.SentAt);
    }

    [Fact]
    public async Task ProcessAsync_FutureReminder_IsNotTouched()
    {
        await factory.ResetDatabaseAsync();
        var reminder = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(7));
        await SeedReminder(reminder);

        await RunProcessor();

        var result = await GetReminder(reminder.Id);
        Assert.Null(result!.SentAt);
    }

    [Fact]
    public async Task ProcessAsync_AlreadySentReminder_IsNotReprocessed()
    {
        await factory.ResetDatabaseAsync();
        var alreadySentAt = DateTime.UtcNow.AddDays(-1);
        var reminder = MakeReminder(scheduledFor: DateTime.UtcNow.Date, sentAt: alreadySentAt);
        await SeedReminder(reminder);

        await RunProcessor();

        var result = await GetReminder(reminder.Id);
        // SentAt should still be the original value, not updated
        Assert.Equal(alreadySentAt, result!.SentAt);
    }

    [Fact]
    public async Task ProcessAsync_EmptyDatabase_DoesNotThrow()
    {
        await factory.ResetDatabaseAsync();

        var ex = await Record.ExceptionAsync(RunProcessor);

        Assert.Null(ex);
    }

    [Fact]
    public async Task ProcessAsync_MixedReminders_OnlySendsNormalOnes()
    {
        await factory.ResetDatabaseAsync();

        var today = MakeReminder(scheduledFor: DateTime.UtcNow.Date);
        var yesterday = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(-1));
        var missed = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(-5));
        var future = MakeReminder(scheduledFor: DateTime.UtcNow.Date.AddDays(3));

        await SeedReminder(today);
        await SeedReminder(yesterday);
        await SeedReminder(missed);
        await SeedReminder(future);

        await RunProcessor();

        Assert.NotNull((await GetReminder(today.Id))!.SentAt);
        Assert.NotNull((await GetReminder(yesterday.Id))!.SentAt);
        Assert.Null((await GetReminder(missed.Id))!.SentAt);
        Assert.Null((await GetReminder(future.Id))!.SentAt);
    }
    
    #region Helpers
    private async Task SeedReminder(Reminder reminder)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();
    }

    private async Task<Reminder?> GetReminder(Guid id)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Reminders.FindAsync(id);
    }

    private async Task RunProcessor()
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IReminderProcessor>();
        await processor.ProcessAsync();
    }

    private static Reminder MakeReminder(DateTime scheduledFor, DateTime? sentAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = ReminderType.Rsvp,
        TargetId = Guid.NewGuid(),
        Value = 1,
        Unit = ReminderUnit.Weeks,
        ScheduledFor = scheduledFor,
        SentAt = sentAt,
        CreatedAt = DateTime.UtcNow
    };
    #endregion
}
