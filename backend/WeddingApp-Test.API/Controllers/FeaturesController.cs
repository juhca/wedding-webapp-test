using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.DTO.Modules;

namespace WeddingApp_Test.API.Controllers;

/// <summary>
/// Public endpoint that tells the frontend which modules are licensed for this deployment.
/// No [Authorize] here — this must be callable before the user logs in,
/// so the app knows which nav items to show on the login/landing page.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeaturesController(IOptions<ModulesOptions> modules) : ControllerBase
{
    /// <summary>
    /// Returns the licensed modules for this deployment.
    /// The frontend calls this once on startup (via APP_INITIALIZER) and uses the result
    /// to conditionally render navigation and guard routes.
    /// </summary>
    [HttpGet]
    public IActionResult GetModules()
    {
        var m = modules.Value;
        return Ok(new ModulesDto
        {
            Gifts = m.Gifts,
            Rsvp = m.Rsvp,
            Reminders = m.Reminders
        });
    }
}
