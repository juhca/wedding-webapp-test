using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO.Reminder;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController(IReminderService reminderService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Add a reminder for a gift reservation (must have an active reservation)
    /// </summary>
    [HttpPost("gifts/{giftId}")]
    [ProducesResponseType(typeof(ReminderDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddGiftReminder(Guid giftId, [FromBody] AddReminderDto dto)
    {
        try
        {
            var reminder = await reminderService.AddGiftReminderAsync(giftId, CurrentUserId, dto);

            return CreatedAtAction(nameof(GetGiftReminders), new { giftId }, reminder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get all reminders for your gift reservation
    /// </summary>
    [HttpGet("gifts/{giftId}")]
    [ProducesResponseType(typeof(IEnumerable<ReminderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetGiftReminders(Guid giftId)
    {
        try
        {
            var reminders = await reminderService.GetGiftRemindersAsync(giftId, CurrentUserId);
            
            return Ok(reminders);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Add a reminder for your RSVP (must have submitted an RSVP)
    /// </summary>
    [HttpPost("rsvp")]
    [ProducesResponseType(typeof(ReminderDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddRsvpReminder([FromBody] AddReminderDto dto)
    {
        try
        {
            var reminder = await reminderService.AddRsvpReminderAsync(CurrentUserId, dto);
            
            return CreatedAtAction(nameof(GetRsvpReminders), reminder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get all reminders for your RSVP
    /// </summary>
    [HttpGet("rsvp")]
    [ProducesResponseType(typeof(IEnumerable<ReminderDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetRsvpReminders()
    {
        try
        {
            var reminders = await reminderService.GetRsvpRemindersAsync(CurrentUserId);
            
            return Ok(reminders);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a reminder (must be the owner)
    /// </summary>
    [HttpDelete("{reminderId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteReminder(Guid reminderId)
    {
        try
        {
            await reminderService.DeleteReminderAsync(reminderId, CurrentUserId);
            
            return Ok(new { message = "Reminder deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
