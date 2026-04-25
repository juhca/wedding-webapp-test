using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Unit tests for <see cref="LiquidRenderer"/>.
/// Verifies variable substitution, conditional logic, graceful handling
/// of missing variables, and rejection of malformed templates.
/// </summary>
[Trait("Category", "LiquidRenderer Unit Tests")]
public class LiquidRendererTests
{
    private readonly LiquidRenderer _renderer = new();
    
    [Fact]
    public async Task SimpleVariable_IsReplaced()
    {
        // Arrange
        const string template = "Hello {{ User.FirstName }}!";
        
        // Act
        var result = await _renderer.RenderAsync(template,
            new()
            {
                ["User"] = new { FirstName = "Anna" }
            });
        
        // Assert
        Assert.Equal("Hello Anna!", result);
    }

    [Fact]
    public async Task Conditional_Works()
    {
        // Arrange
        const string template = "{% if Rsvp.IsAttending %}Coming{% else %}Not coming{% endif %}";

        // Act
        var result = await _renderer.RenderAsync(template, 
            new()
            {
                ["Rsvp"] = new { IsAttending = true }
            });

        // Assert
        Assert.Equal("Coming", result);
    }

    [Fact]
    public async Task MissingVariable_RendersEmpty()
    {
        // Arrange — Fluid renders missing variables as empty string by default, no crash expected
        const string template = "Hello {{ User.FirstName }}!";

        // Act
        var result = await _renderer.RenderAsync(template,
            new Dictionary<string, object?>());

        // Assert
        Assert.Equal("Hello !", result);
    }

    [Fact]
    public async Task InvalidTemplate_Throws()
    {
        // Arrange
        const string template = "{% if unclosed";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _renderer.RenderAsync(template, new()));
    }
}