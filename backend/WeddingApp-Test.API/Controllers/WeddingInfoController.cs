using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeddingInfoController(IWeddingInfoService weddingInfoService, IUserService userService) : ControllerBase
{
    /// <summary>
    /// Get wedding info based on authentication and role
    /// Returns filtered data based on user access level:
    /// - Not logged in: Basic public info only
    /// - Limited Experience: Public + ceremony info
    /// - Full Experience: Everything except admin stats
    /// - Admin: Complete information including statistics
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeddingInfo()
    {
        UserRole? userRole = null;
        // check if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await userService.GetUserAsync(userId);
                if (user is null)
                {
                    
                }
                userRole = user?.Role;
            }
        }

        var info = await weddingInfoService.GetWeddingInfoAsync(userRole);
        return Ok();
    }
    
}