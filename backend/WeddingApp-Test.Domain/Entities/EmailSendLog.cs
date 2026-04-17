namespace WeddingApp_Test.Domain.Entities;

public class EmailSendLog
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;

    public Guid UserId { get; set; }

    // True = outbox record was created successfully (dispatch intent logged)
    // This is NOT the same as "email was delivered" — see EmailOutbox.Status for delivery result
    public bool Dispatched { get; set; }
    public string? Error { get; set; }

    public DateTime DispatchedAt { get; set; }
}