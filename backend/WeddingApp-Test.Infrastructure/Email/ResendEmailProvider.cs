using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Email;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.Infrastructure.Email;

public class ResendEmailProvider(IHttpClientFactory httpClientFactory, IOptions<EmailOptions> options, ILogger<ResendEmailProvider> logger) : IEmailProvider
{
    public string Name => "Resend";

    public async Task SendAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string htmlBody, string? plainTextBody, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Resend");
        var payload = new
        {
            from = $"{fromName} <{fromEmail}>",
            to = new[] { toEmail },
            subject = subject,
            html = htmlBody,
            text = plainTextBody
        };
        var response = await client.PostAsJsonAsync("https://api.resend.com/emails", payload, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Resend returned {Status}: {Body}", response.StatusCode, body);
            throw new EmailDeliveryException($"Resend API returned {(int)response.StatusCode}: {body}");
        }
    }
}