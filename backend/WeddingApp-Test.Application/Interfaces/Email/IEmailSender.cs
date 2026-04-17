namespace WeddingApp_Test.Application.Interfaces.Email;

public interface IEmailSender
{
    /// <summary>
    /// The high-level sender. 
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="toName"></param>
    /// <param name="subject"></param>
    /// <param name="htmlBody"></param>
    /// <param name="plainTextBody"></param>
    /// <param name="ct"></param>
    /// <returns> Returns true on success, false if ALL providers failed (does not throw). </returns>
    Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null, CancellationToken ct = default);
}