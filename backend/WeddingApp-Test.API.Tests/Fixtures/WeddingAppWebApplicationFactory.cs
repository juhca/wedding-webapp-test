using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Fixtures;

/// <summary>
/// Custom factory for creating test instances of the web application.
/// This sets up an isolated environment with an in-memory database for each test.
/// IEmailProvider is replaced with a no-op stub so no real emails are sent during tests.
/// </summary>
public class WeddingAppWebApplicationFactory : WebApplicationFactory<Program>
{
    // DB name is stored as a field, so it's created once per factory instance
    // This ensures all DbContext instances (seeding, HTTP requests, etc.) share the SAME in-memory database.
    //   \-> If we used Guid.NewGuid() directly in UseInMemoryDatabase(), it would create a NEW database
    //      every time a DbContext is instantiated (seeding, each HTTP request, etc.)
    private readonly string _dbName = $"InMemoryTestDb_{Guid.NewGuid()}";

    /// <summary>
    /// Records all emails that would have been sent during the test run.
    /// Inspect this in tests to verify email-sending behaviour.
    /// </summary>
    public List<SentEmailRecord> SentEmails { get; } = [];
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // SET environment to Testing ~ to not execute data seeding
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            // Each test will get a unique database name to avoid conflicts
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Replace IEmailProvider with a no-op stub — no real emails sent in tests
            // Both the keyed ("resend", "smtp") and unkeyed registrations are replaced.
            RemoveAllEmailProviders(services);
            var stub = new StubEmailProvider(SentEmails);
            services.AddSingleton<IEmailProvider>(stub);
            services.AddKeyedSingleton<IEmailProvider>("resend", stub);
            services.AddKeyedSingleton<IEmailProvider>("smtp", stub);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
    
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Remove all data
        db.Users.RemoveRange(db.Users);
        db.Rsvps.RemoveRange(db.Rsvps);
        db.GuestCompanions.RemoveRange(db.GuestCompanions);
        db.WeddingInfo.RemoveRange(db.WeddingInfo);
        db.Gifts.RemoveRange(db.Gifts);
        db.GiftReservations.RemoveRange(db.GiftReservations);
        
        await db.SaveChangesAsync();
    }

    private static void RemoveAllEmailProviders(IServiceCollection services)
    {
        var toRemove = services
            .Where(d => d.ServiceType == typeof(IEmailProvider))
            .ToList();
        foreach (var d in toRemove) services.Remove(d);
    }
}

/// <summary>Captures emails instead of sending them — for use in tests.</summary>
public class StubEmailProvider(List<SentEmailRecord> log) : IEmailProvider
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        log.Add(new SentEmailRecord(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

public record SentEmailRecord(string To, string Subject, string HtmlBody);