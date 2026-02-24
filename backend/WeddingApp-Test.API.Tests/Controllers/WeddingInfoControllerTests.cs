using WeddingApp_Test.API.Tests.Fixtures;

namespace WeddingApp_Test.API.Tests.Controllers;

[Trait("Category", "Wedding Info Integration Tests")]
public class WeddingInfoControllerTests
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
        // Arrange
        // Assert
    }
}