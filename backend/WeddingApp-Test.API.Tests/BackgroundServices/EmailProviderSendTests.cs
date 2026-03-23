using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeddingApp_Test.API.Tests.Fixtures;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.Tests.BackgroundServices;

[Trait("Category", "EmailProvider Tests")]
[Collection("Sequential")]
public class EmailProviderSendTests(WeddingAppWebApplicationFactory factory) : IClassFixture<WeddingAppWebApplicationFactory>
{
    [Fact]
    public async Task SendAsync_ResendProvider_SendsEmail()
    {
		var (enabled, recipientEmail) = GetSettings();
		if (!enabled)
        {
            return;
        }

        await SendViaProvider("Resend", recipientEmail, "Provider test email from Resend pipi pupu");
    }

    [Fact]
    public async Task SendAsync_SmtpProvider_SendsEmail()
    {
        var (enabled, recipientEmail) = GetSettings();

        if (!enabled)
        {
            return;
        }

        await SendViaProvider("SMTP", recipientEmail, "Provider test email from SMTP");
    }

    private async Task SendViaProvider(string providerName, string recipientEmail, string subject)
    {
        using var scope = factory.Services.CreateScope();
        var providers = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmailProvider>>();
        var provider = providers.FirstOrDefault(p => p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(provider);

        await provider!.SendAsync(recipientEmail, new ProviderTestEmailMessage(subject));
    }

    private (bool, string) GetSettings()
    {
        using var scope = factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var enabled = configuration.GetValue<bool>("EmailProviderTests:Enabled");
        var recipientEmail = configuration["EmailProviderTests:RecipientEmail"];

        if (enabled && string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new InvalidOperationException(
                "Set EmailProviderTests:RecipientEmail in configuration when EmailProviderTests:Enabled is true.");
        }

        return (enabled, recipientEmail ?? string.Empty);
    }

    private sealed class ProviderTestEmailMessage(string subject) : EmailMessage
    {
        public override string Subject { get; } = subject;
        public override string Body { get; } = $"Email provider connectivity test sent at {DateTime.UtcNow:O}.";
    }
}
