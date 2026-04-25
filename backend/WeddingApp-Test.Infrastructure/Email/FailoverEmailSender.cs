using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.Interfaces.Email;

namespace WeddingApp_Test.Infrastructure.Email;

public class FailoverEmailSender(IEnumerable<IEmailProvider> providers, IOptions<EmailOptions> options, ILogger<FailoverEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null, CancellationToken ct = default)
    {
        var opt = options.Value;

        foreach (var emailProvider in providers)
        {
            try
            {
                await emailProvider.SendAsync(opt.FromEmail, opt.FromName, toEmail, toName, subject, htmlBody, plainTextBody, ct);
                
                logger.LogInformation($"Email sent successfully via {emailProvider.Name} to {toEmail}");
                
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Provider {Provider} failed to send email to {Email}", emailProvider.Name, toEmail);
            }
        }
        
        logger.LogError($"All email providers failed for {toEmail}");

        return false;
    }
}