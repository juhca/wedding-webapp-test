using System.Net;
using System.Net.Http.Json;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.Application.DTO.Modules;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "FeaturesController Integration Tests")]
public class FeaturesControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    [Fact]
    public async Task GetFeatures_NoAuth_ReturnsOkWithModuleStatus()
    {
        // Arrange — no token, no seeding needed
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Features");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modules = await response.Content.ReadFromJsonAsync<ModulesDto>();
        Assert.NotNull(modules);
        // Defaults from appsettings: Gifts=true, Rsvp=true, Reminders=false
        Assert.True(modules.Gifts);
        Assert.True(modules.Rsvp);
        Assert.True(modules.Reminders);
    }

    [Fact]
    public async Task GetFeatures_WhenGiftsDisabled_StillReturnsOk()
    {
        // Verify FeaturesController itself is never blocked by [RequiresModule]
        var client = factory
            .WithModules(new() { ["Modules:Gifts"] = "false" })
            .CreateClient();

        var response = await client.GetAsync("/api/Features");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modules = await response.Content.ReadFromJsonAsync<ModulesDto>();
        Assert.NotNull(modules);
        Assert.False(modules.Gifts); // reflects the overridden config
    }
}
