namespace WeddingApp_Test.Application.Configuration;

public class ResendOptions
{
    public const string SectionName = "Email:Resend";
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}
