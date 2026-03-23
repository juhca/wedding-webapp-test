namespace WeddingApp_Test.Application.Email;

/// <summary>
/// Thrown by IEmailProvider implementations when sending fails.
/// IsPermanent = true means retrying or switching provider won't help (e.g. invalid address, bad API key).
/// IsPermanent = false means the failure is transient (network issue, 5xx) and another provider may succeed.
/// </summary>
public class EmailProviderException(string message, bool isPermanent = false, Exception? inner = null) : Exception(message, inner)
{
    public bool IsPermanent { get; } = isPermanent;
}
