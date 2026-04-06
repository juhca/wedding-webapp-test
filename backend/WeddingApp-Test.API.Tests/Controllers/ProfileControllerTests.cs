using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.User;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "ProfileController Integration Tests")]
[Collection("Sequential")]
public class ProfileControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProfile_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        await factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/Profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WhenAuthenticated_ReturnsCurrentUserInfo()
    {
        await factory.ResetDatabaseAsync();

        var accessCode = "PROFILE1";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            guest.FirstName = "Jana";
            guest.LastName = "Novak";
            guest.Email = "jana@example.com";
            db.Users.Add(guest);
        });

        _client.DefaultRequestHeaders.Authorization = await LoginAsGuest(accessCode);

        var response = await _client.GetAsync("/api/Profile");

        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(profile);
        Assert.Equal("Jana", profile.FirstName);
        Assert.Equal("Novak", profile.LastName);
        Assert.Equal("jana@example.com", profile.Email);
    }

    [Fact]
    public async Task UpdateMyEmail_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        await factory.ResetDatabaseAsync();

        var response = await _client.PatchAsJsonAsync("/api/Profile/email", new UpdateUserEmailRequest("new@example.com"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyEmail_WithValidEmail_ReturnsOkAndUpdatesEmail()
    {
        await factory.ResetDatabaseAsync();

        var accessCode = "PROFILE2";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            guest.Email = "old@example.com";
            db.Users.Add(guest);
        });

        _client.DefaultRequestHeaders.Authorization = await LoginAsGuest(accessCode);

        var response = await _client.PatchAsJsonAsync("/api/Profile/email", new UpdateUserEmailRequest("new@example.com"));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(updated);
        Assert.Equal("new@example.com", updated.Email);
    }

    [Fact]
    public async Task UpdateMyEmail_WithDuplicateEmail_ReturnsConflict()
    {
        await factory.ResetDatabaseAsync();

        var accessCode = "PROFILE3";
        var takenEmail = "taken@example.com";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            guest.Email = "myemail@example.com";
            db.Users.Add(guest);

            var otherGuest = TestDataBuilder.CreateGuestUser("OTHER01", UserRole.FullExperience);
            otherGuest.Email = takenEmail;
            db.Users.Add(otherGuest);
        });

        _client.DefaultRequestHeaders.Authorization = await LoginAsGuest(accessCode);

        var response = await _client.PatchAsJsonAsync("/api/Profile/email", new UpdateUserEmailRequest(takenEmail));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyEmail_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();

        var accessCode = "PROFILE4";
        await SeedDatabase(db =>
        {
            var guest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(guest);
        });

        _client.DefaultRequestHeaders.Authorization = await LoginAsGuest(accessCode);

        var response = await _client.PatchAsJsonAsync("/api/Profile/email", new UpdateUserEmailRequest("not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #region Helpers
    private async Task<System.Net.Http.Headers.AuthenticationHeaderValue> LoginAsGuest(string accessCode)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", new GuestLoginRequest(accessCode));
        loginResponse.EnsureSuccessStatusCode();
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(result?.Token);
        return new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
    }

    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seedAction(db);
        await db.SaveChangesAsync();
    }
    #endregion
}
