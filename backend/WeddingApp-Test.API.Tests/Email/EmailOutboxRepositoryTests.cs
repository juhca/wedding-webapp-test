using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Integration tests for <see cref="IEmailOutboxRepository"/>.
/// Verifies that outbox records are persisted and queried correctly against a real database.
/// </summary>
[Trait("Category", "EmailOutboxRepository Integration Tests")]
public class EmailOutboxRepositoryTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    /// <summary>
    /// Verifies that a newly added outbox record can be retrieved by ID
    /// and defaults to <see cref="EmailStatus.Pending"/>.
    /// </summary>
    [Fact]
    public async Task AddAndGetById_ReturnsCorrectRecord()
    {
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
        
        // Arrange – build a minimal valid outbox record
        var outbox = new EmailOutbox
        {
            Id = Guid.NewGuid(),
            ToEmail = "guest@example.com",
            ToName = "Guest",
            Subject = "Test",
            HtmlBody = "<p>Hello</p>",
            EmailType = "test.event",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act – persist and retrieve by primary key
        await repo.AddAsync(outbox);
        await repo.SaveChangesAsync();
        
        var result = await repo.GetByIdAsync(outbox.Id);

        // Assert – record exists with correct email and the default Pending status
        Assert.NotNull(result);
        Assert.Equal("guest@example.com", result.ToEmail);
        Assert.Equal(EmailStatus.Pending, result.Status);
    }
    
    /// <summary>
    /// Verifies that <see cref="IEmailOutboxRepository.GetPendingRetryableAsync"/> returns only
    /// records that are Pending and whose retry window has elapsed or is unset.
    /// </summary>
    [Fact]
    public async Task GetPendingRetryable_ReturnsOnlyCorrectRecords()
    {
        // Reset to a clean state so leftover records from other tests don't interfere
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
        var now = DateTime.UtcNow;

        // Arrange – three records that each represent a distinct filtering scenario

        // Should be returned — Pending with no NextRetryAt, immediately eligible
        var ready = new EmailOutbox { Id = Guid.NewGuid(), ToEmail = "a@a.com", ToName = "A", Subject = "S", HtmlBody = "B", EmailType = "x", CreatedAt = now, Status = EmailStatus.Pending };

        // Should NOT be returned — Pending but NextRetryAt is 24 h in the future
        var future = new EmailOutbox { Id = Guid.NewGuid(), ToEmail = "b@b.com", ToName = "B", Subject = "S", HtmlBody = "B", EmailType = "x", CreatedAt = now, Status = EmailStatus.Pending, NextRetryAt = now.AddHours(24) };

        // Should NOT be returned — already marked Sent, no longer retryable
        var sent = new EmailOutbox { Id = Guid.NewGuid(), ToEmail = "c@c.com", ToName = "C", Subject = "S", HtmlBody = "B", EmailType = "x", CreatedAt = now, Status = EmailStatus.Sent };

        await repo.AddAsync(ready);
        await repo.AddAsync(future);
        await repo.AddAsync(sent);
        await repo.SaveChangesAsync();

        // Act – query retryable records as of now
        var results = (await repo.GetPendingRetryableAsync(now)).ToList();

        // Assert – only the immediately-eligible record should be returned
        Assert.Single(results);
        Assert.Equal(ready.Id, results[0].Id);
    }
}