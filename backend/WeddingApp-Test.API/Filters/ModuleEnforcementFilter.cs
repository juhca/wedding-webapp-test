using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using WeddingApp_Test.API.Attributes;
using WeddingApp_Test.Application.Configuration;

namespace WeddingApp_Test.API.Filters;

/// <summary>
/// Global authorization filter that enforces module licensing.
/// Registered in Program.cs via options.Filters.AddService, so it runs on every request.
///
/// How it works:
///   1. Checks whether the target controller/action has [RequiresModule] on it.
///   2. If it does, asks ModulesOptions whether that module is enabled in config.
///   3. If disabled → short-circuits with 403 Forbidden before [Authorize] gets a chance to run.
///      (IAsyncAuthorizationFilter runs earlier in the pipeline than [Authorize] for authenticated users.)
///   4. If enabled → does nothing, request continues normally.
///
/// Note: for unauthenticated requests, [Authorize] in the authorization middleware fires first
/// and returns 401. The module check only fires after authentication succeeds.
/// </summary>
public class ModuleEnforcementFilter(IOptions<ModulesOptions> modules) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Look for [RequiresModule] on the controller class or the specific action method
        var attr = context.ActionDescriptor.EndpointMetadata
            .OfType<RequiresModuleAttribute>()
            .FirstOrDefault();

        // No attribute = not a licensed module = always allowed through
        if (attr is not null && !modules.Value.IsEnabled(attr.ModuleName))
        {
            // Short-circuit: set a result here so the rest of the pipeline is skipped
            context.Result = new ObjectResult($"Module '{attr.ModuleName}' is not licensed.")
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
