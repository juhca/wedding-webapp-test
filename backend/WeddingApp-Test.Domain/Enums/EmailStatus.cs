namespace WeddingApp_Test.Domain.Enums;

public enum EmailStatus
{
    Pending, // queued ~ not sent yet
    Sent, 
    Failed // gave up after max retries
}