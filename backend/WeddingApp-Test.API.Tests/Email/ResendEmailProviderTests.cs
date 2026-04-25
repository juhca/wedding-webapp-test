using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Unit tests for <see cref="ResendEmailProvider"/>.
/// Verifies that the provider correctly handles HTTP responses from the Resend API,
/// succeeding on 2xx and throwing <see cref="EmailDeliveryException"/> on error codes.
/// </summary>
[Trait("Category", "ResendEmailProvider Unit Tests")]
public class ResendEmailProviderTests
{
    #region Setup
    /// <summary>
    /// Constructs a <see cref="ResendEmailProvider"/> backed by a fake HTTP handler
    /// that always returns the given <paramref name="status"/> and <paramref name="responseBody"/>.
    /// </summary>
    /// <param name="status">The HTTP status code the fake handler will return.</param>
    /// <param name="responseBody">The response body string the fake handler will return.</param>
    /// <returns>A configured <see cref="ResendEmailProvider"/> instance.</returns>
    private static ResendEmailProvider Build(HttpStatusCode status, string responseBody = "{}")
    {
        var handler = new FakeHttpMessageHandler(status, responseBody);
        var client  = new HttpClient(handler);

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Resend")).Returns(client);

        return new ResendEmailProvider(
            factory.Object,
            Options.Create(new EmailOptions { FromEmail = "x@y.com", FromName = "X" }),
            NullLogger<ResendEmailProvider>.Instance);
    }
    #endregion
    
    [Fact]
    public async Task Status200_DoesNotThrow()
    {
        // Arrange
        var provider = Build(HttpStatusCode.OK, """{"id":"123"}""");
        
        // Act & Assert
        await provider.SendAsync("x@y.com", "X", "a@b.com", "A", "sub", "<p>hi</p>", null, default);
        // No exception = pass
    }

    [Fact]
    public async Task Status400_Throws()
    {
        // Arrange
        var provider = Build(HttpStatusCode.BadRequest, """{"message":"bad"}""");
        
        // Act & Assert
        await Assert.ThrowsAsync<EmailDeliveryException>(() =>
            provider.SendAsync("x@y.com", "X", "a@b.com", "A", "sub", "<p>hi</p>", null, default));
    }

    [Fact]
    public async Task Status500_Throws()
    {
        // Arrange
        var provider = Build(HttpStatusCode.InternalServerError, "error");
        
        // Act & Assert
        await Assert.ThrowsAsync<EmailDeliveryException>(() =>
            provider.SendAsync("x@y.com", "X", "a@b.com", "A", "sub", "<p>hi</p>", null, default));
    }
    
    
    #region Helper functions
    // Fake HTTP handler that always returns a fixed response
    /// <summary>
    /// A minimal <see cref="HttpMessageHandler"/> stub that short-circuits all HTTP requests,
    /// returning a fixed <see cref="HttpStatusCode"/> and body without hitting the network.
    /// </summary>
    private class FakeHttpMessageHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body)
            });
    }
    #endregion
}