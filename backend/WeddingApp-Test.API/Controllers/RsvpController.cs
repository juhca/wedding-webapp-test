using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.API.Attributes;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Controllers;

[RequiresModule(ModuleNames.Rsvp)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RsvpController(IRsvpService rsvpService) : ControllerBase
{
    /// <summary>
    /// Get RSVP of logged-in user
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(RsvpDto), 200)]
    public async Task<IActionResult> GetUserRsvp()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rsvp = await rsvpService.GetUserRsvpAsync(userId);
        
        return Ok(rsvp);
    }
    
    /// <summary>
    /// Create or update my RSVP
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RsvpDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateOrUpdate([FromBody] CreateRsvpDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rsvp = await rsvpService.CreateOrUpdateRsvpAsync(userId, dto);
            
            return Ok(rsvp);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Get RSVP summary (Admin only)
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(RsvpSummaryDto), 200)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await rsvpService.GetSummaryAsync();
        
        return Ok(summary);
    }
    
    /// <summary>
    /// Get all RSVPs with user info (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(IEnumerable<RsvpWithUserDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var rsvps = await rsvpService.GetAllWithUsersAsync();
        
        return Ok(rsvps);
    }
    
    [HttpGet("export/catering")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportForCatering()
    {
        var data = await rsvpService.ExportForCateringAsync();
        var csv = new StringBuilder();
        csv.AppendLine("GuestType,FirstName,LastName,Age,DietaryRestrictions,Notes,MainGuestEmail");

        foreach (var item in data)
        {
            csv.AppendLine($"\"{item.GuestType}\",\"{item.FirstName}\",\"{item.LastName}\"," +
                           $"{item.Age ?? 0}," +
                           $"\"{item.DietaryRestrictions}\",\"{item.Notes}\",\"{item.MainGuestEmail}\"");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        
        return File(bytes, "text/csv", $"catering-export-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}