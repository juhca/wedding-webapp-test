using WeddingApp_Test.API.Tests.Fixtures;

namespace WeddingApp_Test.API.Tests.Controllers;

public class RsvpControllerTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    private readonly HttpClient _client =  factory.CreateClient();

    [Fact]
    public async Task GetRsvps_ReturnsOk()
    {
        // TODO
    }
    // todo more tests
}