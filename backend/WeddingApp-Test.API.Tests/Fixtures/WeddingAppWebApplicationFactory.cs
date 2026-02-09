using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
}