using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// Sends emails via the Resend REST API using raw HttpClient (no SDK dependency).
/// https://resend.com/docs/api-reference/emails/send-email
/// </summary>
public class EmailProviderResend : IEmailProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _fromAddress;
    private readonly string _fromName;

    public EmailProviderResend(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["EmailConfig:ResendApiKey"]!;
        _fromAddress = config["EmailConfig:FromAddress"]!;
        _fromName = config["EmailConfig:FromName"] ?? string.Empty;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var messageId = $"{Guid.NewGuid()}@weddingapp";

        var payload = new
        {
            from = string.IsNullOrEmpty(_fromName) ? _fromAddress : $"{_fromName} <{_fromAddress}>",
            to = new[] { to },
            subject,
            html = htmlBody,
            headers = new Dictionary<string, string> { ["X-Message-Id"] = messageId }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(payload)
        };

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            var statusCode = (int)response.StatusCode;

            // 4xx = permanent (bad API key, invalid address, etc.) — retrying won't help
            var isPermanent = statusCode is >= 400 and < 500;

            throw new EmailProviderException(
                $"Resend API returned {statusCode}: {error}",
                isPermanent);
        }
    }
}
