namespace WeddingApp_Test.Infrastructure.Email;

/// <summary>
/// SMTP provider configuration. Bind from appsettings: EmailConfig:Smtp
/// </summary>
public class SmtpConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string FromName { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
