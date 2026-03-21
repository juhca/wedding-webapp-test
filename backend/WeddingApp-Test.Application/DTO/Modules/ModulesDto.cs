namespace WeddingApp_Test.Application.DTO.Modules;

/// <summary>
/// Returned by GET /api/features. The frontend fetches this on startup
/// and uses the flags to show/hide navigation items and guard routes.
/// Property names are camelCase on the wire (ASP.NET Core default) — e.g. "gifts", "rsvp".
/// </summary>
public class ModulesDto
{
    public bool Gifts { get; set; }
    public bool Rsvp { get; set; }
    public bool Reminders { get; set; }
}
