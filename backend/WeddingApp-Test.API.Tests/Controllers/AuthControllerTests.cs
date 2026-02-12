using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

/// <summary>
/// Integration tests for the AuthController.
/// These tests verify the entire authentication flow from HTTP request to database.
/// </summary>
[Trait("Category", "AuthController Integration Tests")]
public class AuthControllerTests : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly WeddingAppWebApplicationFactory _factory;
    private readonly HttpClient _client;
    
    public AuthControllerTests(WeddingAppWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region AdminLoginTests

    [Fact]
    public async Task AdminLogin_WithValidCredentials_ReturnsOkWithTokens()
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
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.RefreshToken);
        Assert.NotEmpty(loginResponse.RefreshToken.Token);
        Assert.True(loginResponse.RefreshToken.Expires > DateTime.UtcNow);
    }


	[Fact]
	public async Task GuestLogin_GetAllUsers_ReturnsNotAuth()
	{
        // Arrange
        // Seed the database with a test guest user
        var accessCode = "TestAccessCode";

		await SeedDatabase(db =>
		{
			var user = TestDataBuilder.CreateGuestUser(accessCode);
			db.Users.Add(user);
		});

        var loginRequest = new GuestLoginRequest(accessCode);

		// Act
        // 1. Login first to get the token
		var loginResponse = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
		loginResponse.EnsureSuccessStatusCode();
        

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
		Assert.NotNull(loginResult);
		Assert.NotNull(loginResult.Token);

        // 2. Try to access GetAll with token from guest
        var getAllRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Users/GetAll");
        getAllRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

        var getAllResponse = await _client.SendAsync(getAllRequest);

		// Assert - Guest should not be authorized to get all users
		Assert.Equal(HttpStatusCode.Forbidden, getAllResponse.StatusCode);
	}

	#endregion


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