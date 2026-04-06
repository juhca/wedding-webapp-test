using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Email;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Email;

[Trait("Category", "EmailDispatchService Integration Tests")]
[Collection("Sequential")]
public class EmailDispatchServiceTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    [Fact]
    public async Task DispatchEventAsync_WithMatchingActiveTemplate_SendsEmail()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event");
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Single(emailService.SentEmails);
        Assert.Equal(user.Email, emailService.SentEmails[0].To);
    }

    [Fact]
    public async Task DispatchEventAsync_RendersLiquidSubject_WithUserFirstName()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser("Charlie");
        db.Users.Add(user);
        db.EmailTemplates.Add(MakeTemplate("test.event"));
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Equal("Hello Charlie", emailService.SentEmails[0].Subject);
    }

    [Fact]
    public async Task DispatchEventAsync_CreatesSuccessfulSendLog()
    {
        var (db, _, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event");
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        var log = db.EmailSendLogs.SingleOrDefault(l => l.TemplateId == template.Id && l.UserId == user.Id);
        Assert.NotNull(log);
        Assert.True(log.Succeeded);
        Assert.Null(log.Error);
    }

    [Fact]
    public async Task DispatchEventAsync_WithNoMatchingTemplate_SendsNoEmail()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        db.Users.Add(user);
        db.EmailTemplates.Add(MakeTemplate("other.event"));
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task DispatchEventAsync_WithInactiveTemplate_SendsNoEmail()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event");
        template.IsActive = false;
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task DispatchEventAsync_WhenMaxSendsPerUserReached_SkipsTemplate()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event", maxSends: 1);
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        // Seed an existing successful send log
        db.EmailSendLogs.Add(new EmailSendLog
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            UserId = user.Id,
            SentAt = DateTime.UtcNow.AddDays(-1),
            Succeeded = true
        });
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Empty(emailService.SentEmails);
    }

    [Fact]
    public async Task DispatchEventAsync_WhenMaxSendsNotReached_SendsEmail()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event", maxSends: 2);
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        // Only 1 send so far, limit is 2
        db.EmailSendLogs.Add(new EmailSendLog
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            UserId = user.Id,
            SentAt = DateTime.UtcNow.AddDays(-1),
            Succeeded = true
        });
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Single(emailService.SentEmails);
    }

    [Fact]
    public async Task DispatchEventAsync_WhenSendFails_CreatesFailedSendLog()
    {
        var (db, _, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event");
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();

        var failingEmailService = new FailingEmailService();
        var renderer = new LiquidRenderer();
        var failingSut = new EmailDispatchService(db, failingEmailService, renderer, NullLogger<EmailDispatchService>.Instance);

        await failingSut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        var log = db.EmailSendLogs.SingleOrDefault(l => l.TemplateId == template.Id && l.UserId == user.Id);
        Assert.NotNull(log);
        Assert.False(log.Succeeded);
        Assert.NotNull(log.Error);
    }

    [Fact]
    public async Task DispatchEventAsync_WithMultipleTemplates_SendsAll()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        db.Users.Add(user);
        db.EmailTemplates.AddRange(
            MakeTemplate("test.event"),
            MakeTemplate("test.event")
        );
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Equal(2, emailService.SentEmails.Count);
    }

    [Fact]
    public async Task DispatchEventAsync_FailedPreviousSendDoesNotCountTowardMaxSends()
    {
        var (db, emailService, sut) = await CreateSut();

        var user = MakeUser();
        var template = MakeTemplate("test.event", maxSends: 1);
        db.Users.Add(user);
        db.EmailTemplates.Add(template);
        // Seed a FAILED send log — should not count toward MaxSendsPerUser
        db.EmailSendLogs.Add(new EmailSendLog
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            UserId = user.Id,
            SentAt = DateTime.UtcNow.AddDays(-1),
            Succeeded = false
        });
        await db.SaveChangesAsync();

        await sut.DispatchEventAsync("test.event", user, new Dictionary<string, object?>());

        Assert.Single(emailService.SentEmails);
    }
    
    #region Helpers
    private async Task<(AppDbContext db, CapturingEmailService emailService, EmailDispatchService sut)> CreateSut()
    {
        await factory.ResetDatabaseAsync();

        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = new CapturingEmailService();
        var renderer = new LiquidRenderer();
        var logger = NullLogger<EmailDispatchService>.Instance;
        var sut = new EmailDispatchService(db, emailService, renderer, logger);

        return (db, emailService, sut);
    }

    private static User MakeUser(string firstName = "Alice") => new()
    {
        Id = Guid.NewGuid(),
        FirstName = firstName,
        LastName = "Test",
        Email = $"{firstName.ToLower()}@example.com",
        Role = UserRole.FullExperience,
        RefreshTokens = []
    };

    private static EmailTemplate MakeTemplate(string triggerName, int? maxSends = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Template",
        TriggerName = triggerName,
        Subject = "Hello {{ user.FirstName }}",
        Body = "This is a test email.",
        IsActive = true,
        MaxSendsPerUser = maxSends,
        CreatedAt = DateTime.UtcNow
    };
    #endregion
    
    #region Stubs
    private sealed class CapturingEmailService : IEmailService
    {
        public List<(string To, string Subject, string Body)> SentEmails { get; } = [];

        public Task SendAsync(string recipientEmail, string subject, string body, CancellationToken ct = default)
        {
            SentEmails.Add((recipientEmail, subject, body));
            
            return Task.CompletedTask;
        }
    }

    private sealed class FailingEmailService : IEmailService
    {
        public Task SendAsync(string recipientEmail, string subject, string body, CancellationToken ct = default) => throw new EmailDeliveryException(Guid.Empty, isTransient: true);
    }
    #endregion
}
