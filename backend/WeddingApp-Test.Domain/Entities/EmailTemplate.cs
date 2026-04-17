namespace WeddingApp_Test.Domain.Entities;

public class EmailTemplate
{
    public Guid Id { get; set; }
    
    /// <summary>
    ///  Human-readable label shown in the admin panel
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The event this template responds to, "rsvp.submitted"
    /// </summary>
    public string TriggerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Liquid template for the subject line
    /// </summary>
    public string SubjectTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Liquid template for the HTML email body
    /// </summary>
    public string HtmlBodyTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional Liquid template for the plain-text fallback
    /// </summary>
    public string? PlainTextBodyTemplate { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Max sends to one user. null ~ unlimited, 1 = once only
    /// </summary>
    public int? MaxSendsPerUser { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<EmailSendLog> SendLogs { get; set; } = [];
}