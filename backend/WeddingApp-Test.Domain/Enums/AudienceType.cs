namespace WeddingApp_Test.Domain.Enums;

public enum AudienceType
{
    TriggeredUser,       // only the user who caused the event
    All,                 // all non-admin users
    Attending,           // users with IsAttending = true
    NotAttending,        // users with IsAttending = false
    NoRsvp,              // users who haven't submitted an RSVP yet
    NoGiftReservation,   // users with zero gift reservations
    HasGiftReservation,  // users with at least one gift reservation
    ByRole               // filtered by UserRole (see EmailTemplate.TargetRole)
}
