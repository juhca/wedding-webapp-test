using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO.WeddingInfo;
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
    [ProducesResponseType(typeof(WeddingInfoDto), 200)]
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
                userRole = user?.Role;
            }
        }

        var info = await weddingInfoService.GetWeddingInfoAsync(userRole);
        
        return Ok(info);
    }
    
    /// <summary>
    /// Initialize wedding info (Admin only - one time setup)
    /// </summary>
    [HttpPost("initialize")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(WeddingInfoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> InitializeWeddingInfo([FromBody] WeddingInfoUpdateDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var info = await weddingInfoService.InitializeWeddingInfoAsync(dto, userId);
            
            return Created(string.Empty, info);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update wedding info (Admin only)
    /// </summary>
    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(WeddingInfoDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateWeddingInfo([FromBody] WeddingInfoUpdateDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var info = await weddingInfoService.UpdateWeddingInfoAsync(dto, userId);
            
            return Ok(info);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}