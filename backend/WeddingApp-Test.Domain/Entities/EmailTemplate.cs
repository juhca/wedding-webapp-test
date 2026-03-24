namespace WeddingApp_Test.Domain.Entities;

public class EmailTemplate
{
    public Guid Id { get; set; }

    /// <summary>Human-readable label, e.g. "RSVP Confirmation"</summary>
    public required string Name { get; set; }

    /// <summary>Event key matched at dispatch time, e.g. "rsvp.submitted"</summary>
    public required string TriggerName { get; set; }

    /// <summary>Liquid template string for the email subject</summary>
    public required string Subject { get; set; }

    /// <summary>Liquid template string for the email body (plain text)</summary>
    public required string Body { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Maximum number of times this template can be sent to the same user. Null = unlimited.</summary>
    public int? MaxSendsPerUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmailSendLog> SendLogs { get; set; } = [];
}
