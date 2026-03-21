using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.API.Tests.Helpers;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "Module Enforcement Integration Tests")]
public class ModuleEnforcementTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    [Fact]
    public async Task GiftsController_WhenGiftsDisabled_UnauthenticatedReturns401()
    {
        // [Authorize] in the authorization middleware fires before our filter pipeline,
        // so unauthenticated requests always get 401 regardless of module state.
        // See GiftsController_WhenGiftsDisabled_AdminAlsoGets403 for the authoritative enforcement test.
        var client = factory
            .WithModules(new() { ["Modules:Gifts"] = "false" })
            .CreateClient();

        var response = await client.GetAsync("/api/Gifts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GiftsController_WhenGiftsEnabled_PassesModuleCheckReturnsUnauthorized()
    {
        // Arrange — default config has Gifts=true
        var client = factory.CreateClient();

        // Act — unauthenticated, module is enabled so module filter passes,
        // then [Authorize] fires → 401, not 403
        var response = await client.GetAsync("/api/Gifts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RsvpController_WhenRsvpDisabled_UnauthenticatedReturns401()
    {
        // Same as above — unauthenticated requests get 401 from auth middleware before our filter runs.
        var client = factory
            .WithModules(new() { ["Modules:Rsvp"] = "false" })
            .CreateClient();

        var response = await client.GetAsync("/api/Rsvp/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GiftsController_WhenGiftsDisabled_AdminAlsoGets403()
    {
        // Even admins can't access a disabled module — licensing decision, not a role decision
        var disabledFactory = factory.WithModules(new() { ["Modules:Gifts"] = "false" });
        var client = disabledFactory.CreateClient();


        var email = "admin@wedding.com";
        var password = "SecurePassword123";
        await SeedDatabase(disabledFactory, db =>
        {
            db.Users.Add(TestDataBuilder.CreateAdminUser(email, password));
        });

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/AdminLogin",
            new AdminLoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Act
        var response = await client.GetAsync("/api/Gifts");

        // Assert — 403, not 401: auth passed, module check blocked it
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task SeedDatabase(WebApplicationFactory<Program> f, Action<AppDbContext> seed)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }
}
