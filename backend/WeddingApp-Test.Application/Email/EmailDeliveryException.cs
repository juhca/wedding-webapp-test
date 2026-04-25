namespace WeddingApp_Test.Application.Email;

public class EmailDeliveryException : Exception
{
    public  EmailDeliveryException(string message) : base(message) { }
    public  EmailDeliveryException(string message, Exception inner) : base(message, inner) { }
}