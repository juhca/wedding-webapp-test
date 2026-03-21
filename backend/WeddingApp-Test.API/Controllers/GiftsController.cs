using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.API.Attributes;
using WeddingApp_Test.Application.Configuration;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Controllers;

[RequiresModule(ModuleNames.Gifts)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GiftsController(IGiftService giftService) : ControllerBase
{
    /// <summary>
    /// Get all visible gifts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GiftDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var gifts = await giftService.GetAllVisibleAsync(userId);
        
        return Ok(gifts);
    }
    
    /// <summary>
    /// Get gift by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GiftDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var gift = await giftService.GetByIdAsync(id, userId);
            
            return Ok(gift);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Get my reserved gifts
    /// </summary>
    [HttpGet("my-reservations")]
    [ProducesResponseType(typeof(IEnumerable<GiftDto>), 200)]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gifts = await giftService.GetMyReservedGiftsAsync(userId);
        
        return Ok(gifts);
    }
    
    /// <summary>
    /// Reserve a gift
    /// </summary>
    [HttpPost("{id}/reserve")]
    [ProducesResponseType(typeof(GiftReservationConfirmationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reserve(Guid id, [FromBody] ReserveGiftDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var confirmation = await giftService.ReserveGiftAsync(id, userId, dto);
            
            return Ok(confirmation);
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
    /// Unreserve a gift
    /// </summary>
    [HttpDelete("{id}/reserve")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unreserve(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await giftService.UnreserveGiftAsync(id, userId);
            
            return Ok(new { message = "Gift unreserved successfully" });
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
    /// Create gift (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(GiftDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateGiftDto dto)
    {
        var gift = await giftService.CreateAsync(dto);
        
        return CreatedAtAction(nameof(GetById), new { id = gift.Id }, gift);
    }

    /// <summary>
    /// Update gift (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(GiftDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGiftDto dto)
    {
        try
        {
            var gift = await giftService.UpdateAsync(id, dto);
            
            return Ok(gift);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete gift (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await giftService.DeleteAsync(id);
            
            return Ok(new { message = "Gift deleted successfully" });
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
    /// Import gifts from a JSON array (Admin only)
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ImportGiftsResultDto), 200)]
    public async Task<IActionResult> ImportJson([FromBody] List<CreateGiftDto> dtos)
    {
        var result = await giftService.ImportGiftsAsync(dtos);
        
        return Ok(result);
    }

    /// <summary>
    /// Import gifts from a CSV file (Admin only).
    /// Expected headers: Name, Description, Price, ImageUrl, PurchaseLink, MaxReservations, DisplayOrder, IsVisible
    /// Field values containing commas must be wrapped in double quotes.
    /// </summary>
    [HttpPost("import/csv")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ImportGiftsResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        try
        {
            var result = await giftService.ImportGiftsCsvAsync(file);
            
            return Ok(result);
        }
        catch (InvalidOperationException  ex)
        {
            return BadRequest($"CSV parsing error: {ex.Message}");
        }
    }
}