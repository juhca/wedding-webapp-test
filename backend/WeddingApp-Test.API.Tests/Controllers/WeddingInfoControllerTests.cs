using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.DTO.WeddingInfo;
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
    // TODO(FIX FOR ADMIN IF USER.ITENTIY.IsAUTHENTICATED not works
    public async Task WeddingInfo_ReturnBasicWeddingInfo()
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
        loginResponse.EnsureSuccessStatusCode();
        
        // Act
        var weddingInfoResponse = await _client.GetAsync("/api/WeddingInfo");
        weddingInfoResponse.EnsureSuccessStatusCode();
        
        // Assert
        var weddingInfoResult = await weddingInfoResponse.Content.ReadFromJsonAsync<WeddingInfoDto>();
        Assert.NotNull(weddingInfoResult);
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