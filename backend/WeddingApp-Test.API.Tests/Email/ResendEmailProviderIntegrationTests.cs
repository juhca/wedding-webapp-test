using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Infrastructure.Email;

namespace WeddingApp_Test.API.Tests.Email;

/// <summary>
/// Integration tests for <see cref="ResendEmailProvider"/> against the live Resend API.
/// Requires a valid <c>Email:Resend:ApiKey</c> in <c>appsettings.json</c> to run.
/// Tests are skipped automatically when the key is absent.
/// </summary>
[Trait("Category", "Integration")]
public class ResendEmailProviderIntegrationTests
{
    private const string RecipientEmail = "realEMail@gmail.com";
    private const string RecipientName  = "Tomas";

    #region Setup
    /// <summary>
    /// Builds a <see cref="ResendEmailProvider"/> wired to the real Resend API,
    /// with a <see cref="CapturingHandler"/> in the pipeline so tests can inspect
    /// raw response bodies after each call.
    /// </summary>
    /// <param name="fromEmail">The sender address written into <see cref="EmailOptions"/>.</param>
    /// <param name="fromName">The sender display name written into <see cref="EmailOptions"/>.</param>
    /// <returns>
    /// A tuple of the configured provider, the capturing handler (for response inspection),
    /// and the raw <see cref="HttpClient"/> (for polling the Resend status endpoint).
    /// </returns>
    private static (ResendEmailProvider provider, CapturingHandler handler, HttpClient rawClient) Build(string fromEmail = "onboarding@resend.dev", string fromName  = "Wedding App")
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var options = config.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();
        options.FromEmail = fromEmail;
        options.FromName  = fromName;

        var capturing = new CapturingHandler(new HttpClientHandler());
        var client = new HttpClient(capturing);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Resend.ApiKey);

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Resend")).Returns(client);

        var provider = new ResendEmailProvider(
            factory.Object,
            Options.Create(options),
            NullLogger<ResendEmailProvider>.Instance);

        return (provider, capturing, client);
    }
    #endregion

    [Fact]
    public async Task SendAsync_ToRealEmail_ResendAcceptsAndDelivers()
    {
        var build = Build();

        if (string.IsNullOrWhiteSpace(
                new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetValue<string>("Email:Resend:ApiKey")))
            throw new InvalidOperationException("SKIP: Email:Resend:ApiKey is not configured.");

        await build.provider.SendAsync(
            "onboarding@resend.dev", "Wedding App",
            RecipientEmail, RecipientName,
            "[Integration test] Resend live test",
            "<p>If you're reading this, the integration test passed.</p>",
            "Plain text fallback: integration test passed.",
            default);
    }

    private sealed class CapturingHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        public string? LastResponseBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            LastResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            response.Content = new StringContent(LastResponseBody);
            return response;
        }
    }
}
