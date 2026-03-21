namespace WeddingApp_Test.Application.Configuration;

/// <summary>
/// Binds the "Modules" section from appsettings.json.
/// Each property maps to one feature module that can be toggled per-customer deployment.
/// Defaults: core modules (Gifts, Rsvp) are ON; optional modules (Reminders) are OFF.
/// Changing a value requires a backend restart -> no hot-reload.
/// </summary>
public class ModulesOptions
{
    /// <summary>The key used to locate this section in appsettings.json.</summary>
    public const string SectionName = "Modules";

    public bool Gifts { get; set; } = true;
    public bool Rsvp { get; set; } = true;
    public bool Reminders { get; set; } = false;

    /// <summary>
    /// Returns true if the named module is licensed for this deployment.
    /// Called by ModuleEnforcementFilter on every request to a [RequiresModule] controller.
    /// Unknown module names return false (safe default).
    /// </summary>
    public bool IsEnabled(string moduleName) => moduleName switch
    {
        ModuleNames.Gifts => Gifts,
        ModuleNames.Rsvp => Rsvp,
        ModuleNames.Reminders => Reminders,
        _ => false // unknown modules are off by default
    };
}
