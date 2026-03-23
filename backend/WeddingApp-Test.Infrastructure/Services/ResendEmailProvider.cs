using System.Net;
using System.Net.Http.Headers;
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

    public async Task SendAsync(string recipientEmail, EmailMessage message, CancellationToken ct = default)
    {
        var payload = new
        {
            from = _options.FromAddress,
            to = new[] { recipientEmail },
            subject = message.Subject,
            text = message.Body
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            var isPermanent = IsPermanentFailure(response.StatusCode);

            throw new EmailProviderException($"Resend API returned {(int)response.StatusCode}: {error}", isPermanent);
        }

        logger.LogInformation("{Provider}: email sent to {Recipient}.", Name, recipientEmail);
    }

    private static bool IsPermanentFailure(HttpStatusCode statusCode)
    {
        if ((int)statusCode is < 400 or >= 500)
        {
            return false;
        }

        return statusCode is not HttpStatusCode.TooManyRequests and not HttpStatusCode.RequestTimeout;
    }
}
