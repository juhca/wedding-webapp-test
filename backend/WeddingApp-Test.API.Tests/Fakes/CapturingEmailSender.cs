using System.Collections.Concurrent;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.API.Tests.Fakes;

public class CapturingEmailSender : IEmailSender
{
    public ConcurrentBag<SentEmailRecord> SentEmails { get; } = new();
    public bool ShouldFail { get; set; }
    
    
    public Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null, CancellationToken ct = default)
    {
        if (ShouldFail)
        {
            return Task.FromResult(false);
        }
        SentEmails.Add(new SentEmailRecord(toEmail, subject, htmlBody));
        return Task.FromResult(true);
    }
    
    public void Clear() => SentEmails.Clear();
}

public record SentEmailRecord(string ToEmail, string Subject, string HtmlBody);