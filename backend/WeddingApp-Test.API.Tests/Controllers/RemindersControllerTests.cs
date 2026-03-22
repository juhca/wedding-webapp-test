using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.Reminder;
using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "Reminders Integration Tests")]
[Collection("Sequential")]
public class RemindersControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // Wedding date seeded by factory: 2027-06-19
    // Today is assumed to be before 2027-06-19 for all valid reminder tests.
    // For "past reminder" tests: Value=500, Unit=Days → 2027-06-19 - 500 days ≈ 2026-01-06 (past)

    #region Gift Reminders

    [Fact]
    public async Task AddGiftReminder_ValidInput_Returns201()
    {
        await factory.ResetDatabaseAsync();
        var (token, giftId) = await SetupGuestWithReservedGift();

        var dto = new AddReminderDto { Value = 1, Unit = ReminderUnit.Months };

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReminderDto>();
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Value);
        Assert.Equal(ReminderUnit.Months, result.Unit);
        Assert.False(result.IsSent);
    }

    [Fact]
    public async Task AddGiftReminder_WhenNoReservation_Returns400()
    {
        await factory.ResetDatabaseAsync();
        var (token, _) = await SetupGuestWithReservedGift();

        // Use a random gift id the user hasn't reserved
        var unknownGiftId = Guid.NewGuid();
        var dto = new AddReminderDto { Value = 1, Unit = ReminderUnit.Months };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.PostAsJsonAsync($"/api/reminders/gifts/{unknownGiftId}", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddGiftReminder_WhenScheduledForIsInPast_Returns400()
    {
        await factory.ResetDatabaseAsync();
        var (token, giftId) = await SetupGuestWithReservedGift();

        // 500 days before 2027-06-19 lands before today
        var dto = new AddReminderDto { Value = 500, Unit = ReminderUnit.Days };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddGiftReminder_WhenAtLimit_Returns400()
    {
        await factory.ResetDatabaseAsync();
        var (token, giftId) = await SetupGuestWithReservedGift();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Add 10 reminders (max)
        for (var i = 1; i <= 10; i++)
        {
            var dto = new AddReminderDto { Value = i, Unit = ReminderUnit.Weeks };
            var r = await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}", dto);
            r.EnsureSuccessStatusCode();
        }

        // 11th should fail
        var overflow = new AddReminderDto { Value = 11, Unit = ReminderUnit.Weeks };
        var response = await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}", overflow);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGiftReminders_ReturnsList()
    {
        await factory.ResetDatabaseAsync();
        var (token, giftId) = await SetupGuestWithReservedGift();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}",
            new AddReminderDto { Value = 2, Unit = ReminderUnit.Weeks });
        
        await _client.PostAsJsonAsync($"/api/reminders/gifts/{giftId}",
            new AddReminderDto { Value = 1, Unit = ReminderUnit.Months });

        var response = await _client.GetAsync($"/api/reminders/gifts/{giftId}");
        response.EnsureSuccessStatusCode();

        var reminders = await response.Content.ReadFromJsonAsync<List<ReminderDto>>();
        Assert.NotNull(reminders);
        Assert.Equal(2, reminders.Count);
    }

    #endregion

    #region RSVP Reminders
    [Fact]
    public async Task AddRsvpReminder_ValidInput_Returns201()
    {
        await factory.ResetDatabaseAsync();
        var token = await SetupGuestWithRsvp();

        var dto = new AddReminderDto { Value = 2, Unit = ReminderUnit.Months, Note = "Don't forget!" };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/reminders/rsvp", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReminderDto>();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Value);
        Assert.Equal("Don't forget!", result.Note);
    }

    [Fact]
    public async Task AddRsvpReminder_WhenNoRsvp_Returns400()
    {
        await factory.ResetDatabaseAsync();

        // Seed user but no RSVP
        var accessCode = "NORSVPGUEST";
        await SeedDatabase(db =>
        {
            db.WeddingInfo.Add(WeddingInfoSeed());
            db.Users.Add(TestDataBuilder.CreateGuestUser(accessCode, UserRole.LimitedExperience));
        });

        var token = await LoginAsGuest(accessCode);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new AddReminderDto { Value = 1, Unit = ReminderUnit.Months };
        var response = await _client.PostAsJsonAsync("/api/reminders/rsvp", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRsvpReminders_ReturnsList()
    {
        await factory.ResetDatabaseAsync();
        var token = await SetupGuestWithRsvp();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/reminders/rsvp",
            new AddReminderDto { Value = 1, Unit = ReminderUnit.Months });
        await _client.PostAsJsonAsync("/api/reminders/rsvp",
            new AddReminderDto { Value = 2, Unit = ReminderUnit.Months });

        var response = await _client.GetAsync("/api/reminders/rsvp");
        response.EnsureSuccessStatusCode();

        var reminders = await response.Content.ReadFromJsonAsync<List<ReminderDto>>();
        Assert.NotNull(reminders);
        Assert.Equal(2, reminders.Count);
    }

    #endregion

    #region Delete Reminder

    [Fact]
    public async Task DeleteReminder_ValidOwner_Returns200()
    {
        await factory.ResetDatabaseAsync();
        var token = await SetupGuestWithRsvp();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var addResponse = await _client.PostAsJsonAsync("/api/reminders/rsvp",
            new AddReminderDto { Value = 1, Unit = ReminderUnit.Months });
        addResponse.EnsureSuccessStatusCode();
        var reminder = await addResponse.Content.ReadFromJsonAsync<ReminderDto>();
        Assert.NotNull(reminder);

        var deleteResponse = await _client.DeleteAsync($"/api/reminders/{reminder.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Verify it's gone
        var listResponse = await _client.GetAsync("/api/reminders/rsvp");
        var reminders = await listResponse.Content.ReadFromJsonAsync<List<ReminderDto>>();
        Assert.NotNull(reminders);
        Assert.Empty(reminders);
    }

    [Fact]
    public async Task DeleteReminder_WhenNotFound_Returns404()
    {
        await factory.ResetDatabaseAsync();
        var token = await SetupGuestWithRsvp();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/reminders/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Helpers

    private async Task<(string token, Guid giftId)> SetupGuestWithReservedGift()
    {
        var accessCode = $"GUEST_{Guid.NewGuid():N}";
        var giftId = Guid.Empty;

        await SeedDatabase(db =>
        {
            db.WeddingInfo.Add(WeddingInfoSeed());

            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);

            var gift = new Gift
            {
                Id = Guid.NewGuid(),
                Name = "Test Gift",
                MaxReservations = 5,
                IsVisible = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            giftId = gift.Id;
            db.Gifts.Add(gift);
        });

        var token = await LoginAsGuest(accessCode);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var reserveResponse = await _client.PostAsJsonAsync($"/api/gifts/{giftId}/reserve",
            new ReserveGiftDto { Notes = "test" });
        reserveResponse.EnsureSuccessStatusCode();

        return (token, giftId);
    }

    private async Task<string> SetupGuestWithRsvp()
    {
        var accessCode = $"GUEST_{Guid.NewGuid():N}";

        await SeedDatabase(db =>
        {
            db.WeddingInfo.Add(WeddingInfoSeed());
            db.Users.Add(TestDataBuilder.CreateGuestUser(accessCode, UserRole.LimitedExperience));
        });

        var token = await LoginAsGuest(accessCode);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var rsvpResponse = await _client.PostAsJsonAsync("/api/rsvp",
            new CreateRsvpDto { IsAttending = true });
        rsvpResponse.EnsureSuccessStatusCode();

        return token;
    }

    private async Task<string> LoginAsGuest(string accessCode)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin",
            new GuestLoginRequest(accessCode));
        loginResponse.EnsureSuccessStatusCode();
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        return result!.Token;
    }

    private static WeddingInfo WeddingInfoSeed() => new()
    {
        Id = Guid.NewGuid(),
        BrideName = "Jane", BrideSurname = "Doe",
        GroomName = "John", GroomSurname = "Toe",
        ApproximateDate = "Summer 2027",
        WeddingName = "Test Wedding",
        WeddingDescription = "desc",
        WeddingDate = new DateTime(2027, 6, 19),
        CreatedAt = DateTime.UtcNow
    };

    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seedAction(db);
        await db.SaveChangesAsync();
    }
    #endregion
}
