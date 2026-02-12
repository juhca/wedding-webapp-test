using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "UsersController Integration Tests")]
public class UsersControllerTests : IClassFixture<WeddingAppWebApplicationFactory>
{
	private readonly WeddingAppWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public UsersControllerTests(WeddingAppWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
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
