namespace WeddingApp_Test.API.Attributes;

/// <summary>
/// Marks a controller (or individual action) as belonging to a licensed module.
/// If that module is disabled in appsettings.json, every request to the controller
/// is rejected with 403 Forbidden by ModuleEnforcementFilter — regardless of the user's role.
///
/// Usage:
///   [RequiresModule(ModuleNames.Gifts)]
///   public class GiftsController : ControllerBase { ... }
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresModuleAttribute(string moduleName) : Attribute
{
    public string ModuleName { get; } = moduleName;
}
