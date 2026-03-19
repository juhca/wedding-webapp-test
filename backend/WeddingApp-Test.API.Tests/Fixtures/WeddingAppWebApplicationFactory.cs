using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Fixtures;

/// <summary>
/// Custom factory for creating test instances of the web application.
/// - In-memory database (isolated per factory instance)
/// - IEmailProvider replaced with a no-op stub (no real emails sent)
/// - IEmailDispatchService replaced with a no-op stub (fire-and-forget events don't send in tests)
/// - EmailSchedulerService background service does not run in test environment
/// </summary>
public class WeddingAppWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"InMemoryTestDb_{Guid.NewGuid()}";

    /// <summary>Captures all emails that would have been sent. Inspect in tests.</summary>
    public List<SentEmailRecord> SentEmails { get; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory DB
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));

            // Replace IEmailProvider with a no-op stub
            RemoveAll<IEmailProvider>(services);
            var emailStub = new StubEmailProvider(SentEmails);
            services.AddSingleton<IEmailProvider>(emailStub);
            services.AddKeyedSingleton<IEmailProvider>("resend", emailStub);
            services.AddKeyedSingleton<IEmailProvider>("smtp",   emailStub);

            // Replace IEmailDispatchService with a no-op stub
            RemoveAll<IEmailDispatchService>(services);
            services.AddSingleton<IEmailDispatchService>(new StubEmailDispatchService());

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
    
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        db.Users.RemoveRange(db.Users);
        db.Rsvps.RemoveRange(db.Rsvps);
        db.GuestCompanions.RemoveRange(db.GuestCompanions);
        db.WeddingInfo.RemoveRange(db.WeddingInfo);
        db.Gifts.RemoveRange(db.Gifts);
        db.GiftReservations.RemoveRange(db.GiftReservations);
        db.EmailTemplates.RemoveRange(db.EmailTemplates);
        db.EmailSchedules.RemoveRange(db.EmailSchedules);
        db.EmailSendLogs.RemoveRange(db.EmailSendLogs);
        
        await db.SaveChangesAsync();
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        var toRemove = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in toRemove) services.Remove(d);
    }
}

/// <summary>Records emails instead of sending them.</summary>
public class StubEmailProvider(List<SentEmailRecord> log) : IEmailProvider
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        log.Add(new SentEmailRecord(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

/// <summary>No-op dispatch service — domain events don't trigger emails in tests.</summary>
public class StubEmailDispatchService : IEmailDispatchService
{
    public Task DispatchEventAsync(string eventName, WeddingApp_Test.Domain.Entities.User triggeringUser,
        Dictionary<string, object?> extraContext, CancellationToken ct = default)
        => Task.CompletedTask;
}

public record SentEmailRecord(string To, string Subject, string HtmlBody);