using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

[Trait("Category", "LiquidRenderer Unit Tests")]
public class LiquidRendererTests
{
    private readonly LiquidRenderer _renderer = new();

    [Fact]
    public async Task RenderAsync_WithSimpleVariable_ReturnsRenderedText()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Role = Domain.Enums.UserRole.FullExperience,
            RefreshTokens = []
        };

        var result = await _renderer.RenderAsync(
            "Hello {{ user.FirstName }} {{ user.LastName }}!",
            new Dictionary<string, object?> { ["user"] = user });

        Assert.Equal("Hello Alice Smith!", result);
    }

    [Fact]
    public async Task RenderAsync_WithConditional_RendersCorrectBranch()
    {
        var rsvp = new Rsvp
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsAttending = true,
            CreatedAt = DateTime.UtcNow,
            Companions = []
        };

        var attending = await _renderer.RenderAsync(
            "{% if rsvp.IsAttending %}See you there!{% else %}Sorry you can't make it.{% endif %}",
            new Dictionary<string, object?> { ["rsvp"] = rsvp });

        rsvp.IsAttending = false;

        var notAttending = await _renderer.RenderAsync(
            "{% if rsvp.IsAttending %}See you there!{% else %}Sorry you can't make it.{% endif %}",
            new Dictionary<string, object?> { ["rsvp"] = rsvp });

        Assert.Equal("See you there!", attending);
        Assert.Equal("Sorry you can't make it.", notAttending);
    }

    [Fact]
    public async Task RenderAsync_WithNoVariables_ReturnsTemplateUnchanged()
    {
        var result = await _renderer.RenderAsync(
            "No variables here.",
            new Dictionary<string, object?>());

        Assert.Equal("No variables here.", result);
    }

    [Fact]
    public async Task RenderAsync_WithGiftContext_RendersGiftName()
    {
        var gift = new Gift
        {
            Id = Guid.NewGuid(),
            Name = "Coffee Machine",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _renderer.RenderAsync(
            "You reserved: {{ gift.Name }}",
            new Dictionary<string, object?> { ["gift"] = gift });

        Assert.Equal("You reserved: Coffee Machine", result);
    }

    [Fact]
    public async Task RenderAsync_WithInvalidTemplate_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _renderer.RenderAsync("{% invalid syntax %}", new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task RenderAsync_WithMultipleContextValues_RendersAll()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Jones",
            Email = "bob@example.com",
            Role = WeddingApp_Test.Domain.Enums.UserRole.FullExperience,
            RefreshTokens = []
        };

        var gift = new Gift
        {
            Id = Guid.NewGuid(),
            Name = "Blender",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _renderer.RenderAsync(
            "{{ user.FirstName }} reserved {{ gift.Name }}.",
            new Dictionary<string, object?> { ["user"] = user, ["gift"] = gift });

        Assert.Equal("Bob reserved Blender.", result);
    }
}
