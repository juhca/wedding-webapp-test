using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

public class RsvpControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client =  factory.CreateClient();

    [Fact]
    public async Task GetMyRsvp_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/rsvp/my");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRsvp_WithValidData_ReturnsOk()
    {
        // Arrange
        var accessCode = "LimitedExperienceAccessCode";

        // Seed the database with a test admin user
        await SeedDatabase(db =>
        {
            var guestUser = TestDataBuilder.CreateGuestUser(accessCode, UserRole.LimitedExperience);
            db.Users.Add(guestUser);
        });
        
        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        var createDto = new CreateRsvpDto
        {
            IsAttending = true,
            DietaryRestrictions = "Vegetarian",
            Notes = "Excited!",
            WantsReminder = true,
            Companions =
            [
                new CreateGuestCompanionDto
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Age = 28,
                    DietaryRestrictions = "Vegan"
                }
            ]
        };
        
        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var response = await _client.PostAsJsonAsync("/api/rsvp", createDto);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var rsvpResponse = await response.Content.ReadFromJsonAsync<RsvpDto>();
        
        Assert.NotNull(rsvpResponse);
        Assert.Equal(createDto.WantsReminder, rsvpResponse.WantsReminder);
        Assert.Equal(createDto.IsAttending, rsvpResponse.IsAttending);
        Assert.Equal(createDto.DietaryRestrictions, rsvpResponse.DietaryRestrictions);
        Assert.Equal(createDto.Notes, rsvpResponse.Notes);
        Assert.Equal(createDto.Companions.Count, rsvpResponse.Companions.Count);
        Assert.Equal(createDto.Companions[0].FirstName, rsvpResponse.Companions[0].FirstName);
    }

    [Fact]
    public async Task CreateRsvp_ExceedingMaxCompanions_ReturnsBadRequest()
    {
        // Arrange
        
        // Act
        
        // Assert

    }

    [Fact]
    public async Task GetSummary_AsAdmin_ReturnsStatistics()
    {
        // Arrange
        
        // Act
        
        // Assert

        
    }

    [Fact]
    public async Task ExportCatering_AsAdmin_ReturnsCsvFile()
    {
        // Arrange
        
        // Act
        
        // Assert

        
    }
    
    #region HelperMethods
    /// <summary>
    /// Helper method to seed the database with test data.
    /// Creates a new scope and disposes it properly after seeding.
    /// </summary>
    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        seedAction(db);
        await db.SaveChangesAsync();
    }
    #endregion
}