namespace WeddingApp_Test.Domain.Enums;

public enum TriggerType
{
    OnEvent,             // fires immediately when a domain event occurs
    ScheduledRelative,   // N days before/after a reference date (WeddingDate, RsvpDate...)
    ScheduledAbsolute    // fires on a specific calendar date
}
