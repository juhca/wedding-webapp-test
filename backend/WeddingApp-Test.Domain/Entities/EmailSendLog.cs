namespace WeddingApp_Test.Domain.Entities;

public class EmailSendLog
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool Succeeded { get; set; }

    public string? Error { get; set; }
}
