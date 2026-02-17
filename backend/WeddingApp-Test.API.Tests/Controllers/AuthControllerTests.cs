using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Domain.Enums;
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
    public async Task AdminLogin_WithInvalidEmail_ReturnsUnauthorized()
    {
		// Arrange
		var loginRequest = new AdminLoginRequest("nonexistent@wedding.com", "password");

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);

		// Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username or password", content);
	}

    [Fact]
    public async Task AdminLogin_WithInvalidPassword_ReturnsUnauthorized()
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

		var loginRequest = new AdminLoginRequest(email, "WrongPassword123!");

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);

		// Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

		var loginResponse = await response.Content.ReadAsStringAsync();
		Assert.Contains("Invalid username or password", loginResponse);
	}

    [Fact]
    // TODO(TOMAS): mogoce javim da uporabnik nima ustreznih pravic?
    public async Task AdminLogin_WithNonAdminUser_ReturnsUnauthorized()
    {
		// Arrange - Create a guest user trying to login as admin
		var email = "guest@wedding.com";
		var password = "password";
		var (hash, salt) = TestDataBuilder.CreatePasswordHashAndSalt(password);

		await SeedDatabase(db =>
		{
			var guestUser = TestDataBuilder.CreateAdminUser(email, password);
			guestUser.Role = UserRole.FullExperience; // Not an admin!
			db.Users.Add(guestUser);
		});

		var loginRequest = new AdminLoginRequest(email, password);

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);
        var loginResponse = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Contains("Invalid username or password", loginResponse);
	}


	[Fact]
	public async Task AdminLogin_WithUserWithoutPassword_ReturnsUnauthorized()
    {
		// Arrange
		var email = "admin@wedding.com";
		var password = "SecurePassword123";

		// Seed the database with a test admin user
		await SeedDatabase(db =>
		{
			var admin = TestDataBuilder.CreateAdminUser(email, string.Empty);
			db.Users.Add(admin);
		});

		var loginRequest = new AdminLoginRequest(email, "string.Empdty");

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/AdminLogin", loginRequest);

		// Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

		var loginResponse = await response.Content.ReadAsStringAsync();
		Assert.Contains("Invalid username or password", loginResponse);
	}
	#endregion

	#region GuestLoginTests
	[Fact]
	public async Task GuestLogin_WithValidAccessCode_ReturnsOkWithTokens()
	{
		// Arrange
		var accessCode = "WEDDING2024";

		await SeedDatabase(db =>
		{
			var guest = TestDataBuilder.CreateGuestUser(accessCode);
			db.Users.Add(guest);
		});

		var loginRequest = new GuestLoginRequest(accessCode);

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
		Assert.NotNull(loginResponse);
		Assert.NotNull(loginResponse.Token);
		Assert.NotNull(loginResponse.RefreshToken);
		Assert.NotEmpty(loginResponse.Token);
		Assert.NotEmpty(loginResponse.RefreshToken.Token);
	}

	[Fact]
	public async Task GuestLogin_WithInvalidAccessCode_ReturnsUnauthorized()
	{
		// Arrange
		var loginRequest = new GuestLoginRequest("INVALID_CODE");

		// Act
		var response = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);

		// Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

		var content = await response.Content.ReadAsStringAsync();
		Assert.Contains("Invalid Access Code", content);
	}

	[Fact]
	public async Task GuestLogin_WithDifferentUserRoles_ReturnsTokens()
	{
		// Arrange - Test with FullExperience role
		var accessCode1 = "FULL_ACCESS";
		await SeedDatabase(db =>
		{
			var guest = TestDataBuilder.CreateGuestUser(accessCode1, UserRole.FullExperience);
			db.Users.Add(guest);
		});

		var loginRequest1 = new GuestLoginRequest(accessCode1);

		// Act
		var response1 = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest1);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

		// Arrange - Test with LimitedExperience role
		var accessCode2 = "LIMITED_ACCESS";
		await SeedDatabase(db =>
		{
			var guest = TestDataBuilder.CreateGuestUser(accessCode2, UserRole.LimitedExperience);
			db.Users.Add(guest);
		});

		var loginRequest2 = new GuestLoginRequest(accessCode2);

		// Act
		var response2 = await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest2);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
	}

	#endregion

	#region Integration Scenarios

	[Fact]
	public async Task MultipleLogins_CreateMultipleRefreshTokens()
	{
		// Arrange
		var accessCode = "MULTI_LOGIN";
		Guid userId = Guid.Empty;

		await SeedDatabase(db =>
		{
			var guest = TestDataBuilder.CreateGuestUser(accessCode);
			userId = guest.Id;
			db.Users.Add(guest);
		});

		var loginRequest = new GuestLoginRequest(accessCode);

		// Act - Login multiple times
		await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
		await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);
		await _client.PostAsJsonAsync("/api/Auth/GuestLogin", loginRequest);

		// Assert - Verify multiple refresh tokens were created
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var user = await db.Users
			.Include(u => u.RefreshTokens)
			.FirstOrDefaultAsync(u => u.Id == userId);

		Assert.NotNull(user);
		Assert.True(user.RefreshTokens.Count >= 3,
			$"Expected at least 3 refresh tokens, but found {user.RefreshTokens.Count}");
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