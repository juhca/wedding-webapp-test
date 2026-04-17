using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Domain.Entities;

public class EmailOutbox
{
    public Guid Id { get; set; }
    
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    
    public EmailStatus Status { get; set; } =  EmailStatus.Pending;
    public int AttemptCount { get; set; } = 0;
    public string? LastError { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? NexRetryAt { get; set; }
    
    /// <summary>Matches the event trigger name, e.g. "rsvp.submitted".</summary>
    public string EmailType { get; set; } = string.Empty;

    /// <summary>Optional link to the related RSVP, Gift, User, etc.</summary>
    /// don't know if needed, might delete in the future
    public Guid? RelatedEntityId { get; set; }
    
}