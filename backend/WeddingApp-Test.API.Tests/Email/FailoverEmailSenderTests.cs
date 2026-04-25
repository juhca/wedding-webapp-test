using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Unit tests for <see cref="FailoverEmailSender"/>.
/// Verifies that the sender correctly iterates through providers,
/// falls back on failure, and returns the appropriate result.
/// </summary>
[Trait("Category", "FailoverEmailSender Unit Tests")]
public class FailoverEmailSenderTests
{
    #region Setup
    /// <summary>
    /// Constructs a <see cref="FailoverEmailSender"/> with the given providers
    /// and a minimal email options configuration.
    /// </summary>
    /// <param name="providers">The ordered list of email providers to inject.</param>
    /// <returns>A configured <see cref="FailoverEmailSender"/> instance.</returns>
    private static FailoverEmailSender Build(IEnumerable<IEmailProvider> providers) 
        => new (providers, Options.Create(new EmailOptions{FromEmail = "x@y.com", FromName = "X"}), NullLogger<FailoverEmailSender>.Instance);

    /// <summary>
    /// Creates a mock <see cref="IEmailProvider"/> that completes successfully.
    /// </summary>
    /// <param name="name">The display name returned by <see cref="IEmailProvider.Name"/>.</param>
    /// <returns>A mock whose <c>SendAsync</c> returns <see cref="Task.CompletedTask"/>.</returns>
    private static Mock<IEmailProvider> OkProvider(string name)
    {
        var m = new Mock<IEmailProvider>();
        m.Setup(provider => provider.Name).Returns(name);
        m.Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>())) 
            .Returns(Task.CompletedTask);
        
        return m;
    }
    
    /// <summary>
    /// Creates a mock <see cref="IEmailProvider"/> that always throws
    /// <see cref="EmailDeliveryException"/> to simulate a delivery failure.
    /// </summary>
    /// <param name="name">The display name returned by <see cref="IEmailProvider.Name"/>.</param>
    /// <returns>A mock whose <c>SendAsync</c> throws <see cref="EmailDeliveryException"/>.</returns>
    private static Mock<IEmailProvider> FailProvider(string name)
    {
        var m = new Mock<IEmailProvider>();
        m.Setup(p => p.Name).Returns(name);
        m.Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException("fail"));
        
        return m;
    }
    #endregion
    
    [Fact]
    public async Task SingleProvider_Succeeds_ReturnsTrue()
    {
        // Arrange
        var sender = Build([OkProvider("A").Object]);
        
        // Act
        var result = await sender.SendAsync("a@b.com", "A", "sub", "<p>hi</p>", null, default);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task FirstFails_SecondSucceeds_ReturnsTrue()
    {
        // Arrange
        var first  = FailProvider("A");
        var second = OkProvider("B");
        var sender = Build([first.Object, second.Object]);

        // Act
        var result = await sender.SendAsync("a@b.com", "A", "sub", "<p>hi</p>", null, default);

        // Assert
        Assert.True(result);
        first.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        second.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AllFail_ReturnsFalse()
    {
        // Arrange
        var sender = Build([FailProvider("A").Object, FailProvider("B").Object]);
        
        // Act
        var result = await sender.SendAsync("a@b.com", "A", "sub", "<p>hi</p>", null, default);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ZeroProviders_ReturnsFalse()
    {
        // Arrange
        var sender = Build([]);
        
        // Act
        var result = await sender.SendAsync("a@b.com", "A", "sub", "<p>hi</p>", null, default);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FirstSucceeds_SecondNeverCalled()
    {
        // Arrange
        var first  = OkProvider("A");
        var second = OkProvider("B");
        var sender = Build([first.Object, second.Object]);

        // Act
        await sender.SendAsync("a@b.com", "A", "sub", "<p>hi</p>", null, default);

        // Assert
        second.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}