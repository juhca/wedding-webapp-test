using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fakes;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Integration tests that verify end-to-end email dispatch behaviour triggered by RSVP submission.
/// Exercises the full request pipeline — HTTP → domain event → outbox processor → <see cref="CapturingEmailSender"/> —
/// without hitting a real mail provider.
/// </summary>
[Trait("Category", "Email Event Integration Tests")]
[Collection("Sequential")]
public class EmailEventIntegrationTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    #region Setup
    private CapturingEmailSender GetSender() => factory.Services.GetRequiredService<CapturingEmailSender>();

    // Seed a template directly into the test DB.
    // NOTE: The trigger name must match what RsvpService emits — "rsvp.submited" (one 't') is the current production value.
    private async Task SeedTemplateAsync(string triggerName, bool isActive = true, int? maxSends = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.EmailTemplates.Add(new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Name = triggerName,
            TriggerName = triggerName,
            SubjectTemplate = "Event: {{ User.FirstName }}",
            HtmlBodyTemplate = "<p>body</p>",
            IsActive = isActive,
            MaxSendsPerUser = maxSends,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seedAction(db);
        await db.SaveChangesAsync();
    }

    private async Task<string> LoginGuestAsync(string accessCode)
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", new GuestLoginRequest(accessCode));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return result!.Token;
    }

    private async Task SubmitRsvpAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/rsvp", new CreateRsvpDto { IsAttending = true });
        response.EnsureSuccessStatusCode();
    }
    #endregion

    [Fact]
    public async Task SubmitRsvp_UserHasEmail_EmailQueued()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        GetSender().Clear();
        await SeedTemplateAsync("rsvp.submited");

        var accessCode = "EMAIL_RSVP_GUEST";
        await SeedDatabase(db =>
        {
            var user = TestDataBuilder.CreateGuestUser(accessCode);
            user.Email = "guest@example.com";
            db.Users.Add(user);
        });

        // Act
        var token = await LoginGuestAsync(accessCode);
        await SubmitRsvpAsync(token);

        // Background processor is async — give it time to pick up the outbox record
        await Task.Delay(300);

        // Assert
        var sent = GetSender().SentEmails;
        Assert.Single(sent);
        Assert.Equal("guest@example.com", sent.Single().ToEmail);
    }

    [Fact]
    public async Task SubmitRsvp_UserNoEmail_NoEmailSent()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        GetSender().Clear();
        await SeedTemplateAsync("rsvp.submited");

        var accessCode = "NO_EMAIL_RSVP_GUEST";
        await SeedDatabase(db =>
        {
            var user = TestDataBuilder.CreateGuestUser(accessCode);
            user.Email = string.Empty;
            db.Users.Add(user);
        });

        // Act
        var token = await LoginGuestAsync(accessCode);
        await SubmitRsvpAsync(token);

        await Task.Delay(200);

        Assert.Empty(GetSender().SentEmails);
    }

    [Fact]
    public async Task InactiveTemplate_NoEmailSent()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        GetSender().Clear();
        await SeedTemplateAsync("rsvp.submited", isActive: false);

        var accessCode = "INACTIVE_RSVP_GUEST";
        await SeedDatabase(db => db.Users.Add(TestDataBuilder.CreateGuestUser(accessCode)));

        // Act
        var token = await LoginGuestAsync(accessCode);
        await SubmitRsvpAsync(token);

        await Task.Delay(200);

        // Assert
        Assert.Empty(GetSender().SentEmails);
    }

    [Fact]
    public async Task MaxSendsPerUser1_SecondRsvp_OnlyOneEmail()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        GetSender().Clear();
        await SeedTemplateAsync("rsvp.submited", maxSends: 1);

        var accessCode = "MAX_SENDS_RSVP_GUEST";
        Guid userId = Guid.Empty;
        await SeedDatabase(db =>
        {
            var user = TestDataBuilder.CreateGuestUser(accessCode);
            userId = user.Id;
            db.Users.Add(user);
        });

        // Act
        var token = await LoginGuestAsync(accessCode);

        // First submission — email should be sent
        await SubmitRsvpAsync(token);
        await Task.Delay(300);

        // Delete the RSVP so the second POST also follows the "new RSVP" code path (fires "rsvp.submited" again).
        // Send logs are kept so the dedup check can block the second send.
        await SeedDatabase(db =>
        {
            var rsvps = db.Rsvps.Where(r => r.UserId == userId).ToList();
            db.Rsvps.RemoveRange(rsvps);
        });

        // Second submission — MaxSendsPerUser=1 already reached, should be skipped by dispatch service
        await SubmitRsvpAsync(token);
        await Task.Delay(300);

        // Assert
        Assert.Single(GetSender().SentEmails);
    }
}
