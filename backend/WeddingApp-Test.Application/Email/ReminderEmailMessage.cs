using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Email;

public class ReminderEmailMessage : EmailMessage
{
    public ReminderEmailMessage(Reminder reminder)
    {
        Subject = "Wedding Reminder";
        Body = $"""
            Hello,

            This is a reminder for your wedding event.

            Reminder type: {reminder.Type}
            Scheduled for: {reminder.ScheduledFor:yyyy-MM-dd}
            Note: {reminder.Note ?? "No additional notes."}

            Best regards,
            Wedding App
            """;
    }

    public override string Subject { get; }
    public override string Body { get; }
}
