using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Services;

public class ResendEmailProvider(HttpClient httpClient, IOptions<ResendOptions> options, ILogger<ResendEmailProvider> logger) : IEmailProvider
{
    private const string ResendApiUrl = "https://api.resend.com/emails";
    private readonly ResendOptions _options = options.Value;

    public string Name => "Resend";

    public async Task SendAsync(string recipientEmail, EmailMessage message)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var payload = new
        {
            from = _options.FromAddress,
            to = new[] { recipientEmail },
            subject = message.Subject,
            text = message.Body
        };

        var response = await httpClient.PostAsJsonAsync(ResendApiUrl, payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Resend API returned {(int)response.StatusCode}: {error}");
        }

        logger.LogInformation("{Provider}: email sent to {Recipient}.", Name, recipientEmail);
    }
}
