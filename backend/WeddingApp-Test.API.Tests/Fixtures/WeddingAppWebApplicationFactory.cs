using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fakes;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Fixtures;

/// <summary>
/// Custom factory for creating test instances of the web application.
/// This sets up an isolated environment with an in-memory database for each test.
/// </summary>
public class WeddingAppWebApplicationFactory : WebApplicationFactory<Program>
{
    // DB name is stored as a field, so it's created once per factory instance
    // This ensures all DbContext instances (seeding, HTTP requests, etc.) share the SAME in-memory database.
    //   \-> If we used Guid.NewGuid() directly in UseInMemoryDatabase(), it would create a NEW database
    //      every time a DbContext is instantiated (seeding, each HTTP request, etc.)
    private readonly string _dbName = $"InMemoryTestDb_{Guid.NewGuid()}";
    
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

            // Register fake email sender — replaces the real IEmailSender
            services.AddSingleton<CapturingEmailSender>();
            services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<CapturingEmailSender>());
            
            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Seed WeddingInfo — required for WeddingInfoController tests.
            // DataSeeder is skipped in Testing env, so we seed it here.
            db.WeddingInfo.Add(new WeddingInfo
            {
                Id = Guid.NewGuid(),
                BrideName = "Jane", BrideSurname = "Doe",
                GroomName = "John", GroomSurname = "Toe",
                ApproximateDate = "Summer 2027",
                WeddingName = "Test Wedding",
                WeddingDescription = "Test wedding description",
                WeddingDate = new DateTime(2027, 6, 19),
                PartyLocationName = "Test Party Venue",
                HouseLocationName = "Test House",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        });
        
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Modules:Gifts"] = "true",
                ["Modules:Rsvp"] = "true",
                ["Modules:Reminders"] = "true"
            });
        });
    }
    
    public WebApplicationFactory<Program> WithModules(Dictionary<string, string?> overrides)
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(overrides);
            });
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
        db.EmailOutbox.RemoveRange(db.EmailOutbox);
        db.EmailTemplates.RemoveRange(db.EmailTemplates);
        db.EmailSendLogs.RemoveRange(db.EmailSendLogs);

        await db.SaveChangesAsync();
    }
}