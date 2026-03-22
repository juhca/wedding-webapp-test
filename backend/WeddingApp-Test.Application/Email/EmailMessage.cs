namespace WeddingApp_Test.Application.Email;

public abstract class EmailMessage
{
    public abstract string Subject { get; }
    public abstract string Body { get; }
}
