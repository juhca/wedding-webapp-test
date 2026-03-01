using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.WeddingInfo;
using WeddingApp_Test.Domain.Enums;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "Wedding Info Integration Tests")]
public class WeddingInfoControllerTests : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly WeddingAppWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WeddingInfoControllerTests(WeddingAppWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WeddingInfo_ReturnBasicWeddingInfo()
    {
        // Act
        var weddingInfoResponse = await _client.GetAsync("/api/WeddingInfo");
        weddingInfoResponse.EnsureSuccessStatusCode();
        
        // Assert
        var weddingInfoResult = await weddingInfoResponse.Content.ReadFromJsonAsync<WeddingInfoDto>();
        Assert.NotNull(weddingInfoResult);
        Assert.Null(weddingInfoResult.UserRole); // Non-authenticated users are set as null
        Assert.Null(weddingInfoResult.WeddingDate); // Visible to Lite, Full and Admin
        Assert.Null(weddingInfoResult.LocationParty); // Visible Full + Admin
        Assert.Null(weddingInfoResult.LocationHouse); // Visible only to admin 
    }
    
    [Fact]
    public async Task WeddingInfo_ReturnLimitedExperienceWeddingInfo()
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
        
        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var weddingInfoResponse = await _client.GetAsync("/api/WeddingInfo");
        weddingInfoResponse.EnsureSuccessStatusCode();
        
        // Assert
        var weddingInfoResult = await weddingInfoResponse.Content.ReadFromJsonAsync<WeddingInfoDto>();
        Assert.NotNull(weddingInfoResult);
        Assert.Equal(weddingInfoResult.UserRole, UserRole.LimitedExperience);
        Assert.NotNull(weddingInfoResult.WeddingDate); // Visible to Lite, Full and Admin
        Assert.Null(weddingInfoResult.LocationParty); // Visible Full + Admin
        Assert.Null(weddingInfoResult.LocationHouse); // Visible only to admin 
    }
    
    
    [Fact]
    public async Task WeddingInfo_ReturnFullExperienceWeddingInfo()
    {
        // Arrange
        var accessCode = "FullExperienceAccessCode";

        // Seed the database with a test admin user
        await SeedDatabase(db =>
        {
            var fullExGuest = TestDataBuilder.CreateGuestUser(accessCode, UserRole.FullExperience);
            db.Users.Add(fullExGuest);
        });

        var loginRequest = new GuestLoginRequest(accessCode);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var weddingInfoResponse = await _client.GetAsync("/api/WeddingInfo");
        weddingInfoResponse.EnsureSuccessStatusCode();
        
        // Assert
        var weddingInfoResult = await weddingInfoResponse.Content.ReadFromJsonAsync<WeddingInfoDto>();
        Assert.NotNull(weddingInfoResult);
        Assert.Equal(weddingInfoResult.UserRole, UserRole.FullExperience);
        Assert.NotNull(weddingInfoResult.WeddingDate); // Visible to Lite, Full and Admin
        Assert.NotNull(weddingInfoResult.LocationParty); // Visible Full + Admin
        Assert.Null(weddingInfoResult.LocationHouse); // Visible only to admin 
    }
    
    [Fact]
    public async Task WeddingInfo_ReturnAdminWeddingInfo()
    {
        // Arrange
        var email = "admin@wedding.com";
        var password = "SecurePassword123";

        // Seed the database with a test admin user
        await SeedDatabase(db =>
        {
            var admin = TestDataBuilder.CreateAdminUser(email, password);
            db.Users.Add(admin);
        });

        var loginRequest = new AdminLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.EnsureSuccessStatusCode();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        
        // Act
        // Add the token to the Authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
        var weddingInfoResponse = await _client.GetAsync("/api/WeddingInfo");
        weddingInfoResponse.EnsureSuccessStatusCode();
        
        // Assert
        var weddingInfoResult = await weddingInfoResponse.Content.ReadFromJsonAsync<WeddingInfoDto>();
        Assert.NotNull(weddingInfoResult);
        Assert.Equal(weddingInfoResult.UserRole, UserRole.Admin);
        Assert.NotNull(weddingInfoResult.WeddingDate); // Visible to Lite, Full and Admin
        Assert.NotNull(weddingInfoResult.LocationParty); // Visible Full + Admin
        Assert.NotNull(weddingInfoResult.LocationHouse); // Visible only to admin 
    }
    
    #region HelperMethods
    /// <summary>
    /// Helper method to seed the database with test data.
    /// Creates a new scope and disposes it properly after seeding.
    /// </summary>
    private async Task SeedDatabase(Action<AppDbContext> seedAction)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        seedAction(db);
        await db.SaveChangesAsync();
    }
    #endregion
}