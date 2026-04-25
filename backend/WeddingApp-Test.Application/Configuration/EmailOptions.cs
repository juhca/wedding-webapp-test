namespace WeddingApp_Test.Application.Configuration;

public class EmailOptions
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    
    // Exponential backoff tiers: minutes to wait before each retry (index 0 = after 1st failure)
    public int[] RetryDelayMinutes { get; set; } = [1, 5, 30, 360, 1440]; // 1m > 5m > 30m > 6h > 24h
    public int MaxAttempts { get; set; } = 4; // one attempt per tier
    
    public ResendOptions Resend { get; set; } = new();
    public SmtpOptions Smtp { get; set; } = new();
}

public class ResendOptions
{
    public bool Enabled { get; set; } 
    public string ApiKey { get; set; } = string.Empty;
}

public class SmtpOptions   
{ 
    public bool Enabled { get; set; } 
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587; 
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public bool UseSsl { get; set; } = true; 
}