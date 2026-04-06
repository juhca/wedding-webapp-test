namespace WeddingApp_Test.Application.Email;

public sealed class PlainEmail(string subject, string body) : EmailMessage
{
    public override string Subject { get; } = subject;
    public override string Body { get; } = body;
}
