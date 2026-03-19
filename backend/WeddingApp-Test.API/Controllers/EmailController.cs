using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.API.Services;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController(CountdownImageService countdown, AppDbContext db) : ControllerBase
{
    private static readonly string[] NoCacheHeaders = ["no-cache, no-store, must-revalidate"];

    /// <summary>
    /// Returns a 60-frame animated GIF that counts down to the wedding date.
    /// The unique token parameter (injected per-email) defeats Gmail's URL-level image cache
    /// so each recipient sees a fresh GIF on first open.
    ///
    /// Caching behaviour by client:
    ///   Gmail         — caches per URL; unique token = fresh on first open, frozen on re-open
    ///   Apple Mail    — re-fetches on every open → real-time
    ///   Outlook (desktop) — no animated GIF support; shows first frame only
    ///   Outlook.com   — animated GIFs work ✓
    /// </summary>
    [HttpGet("countdown")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetCountdownGif([FromQuery] string? token)
    {
        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        Response.Headers.Pragma       = "no-cache";
        Response.Headers.Expires      = "0";

        var weddingInfo = await db.WeddingInfo.FirstOrDefaultAsync();
        var targetUtc   = weddingInfo?.WeddingDate ?? DateTime.UtcNow.AddDays(90);

        var gif = countdown.GenerateCountdownGif(targetUtc);
        return File(gif, "image/gif");
    }

    /// <summary>
    /// Returns a static personalised PNG: "Hi {firstName}, see you in {N} days!".
    /// The cb (cache-buster) query param keeps Gmail from showing a stale image.
    /// </summary>
    [HttpGet("guest-message")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetGuestMessage([FromQuery] Guid guestId, [FromQuery] string? cb)
    {
        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        Response.Headers.Pragma       = "no-cache";
        Response.Headers.Expires      = "0";

        var user = await db.Users.FindAsync(guestId);
        if (user is null)
            return NotFound();

        var weddingInfo = await db.WeddingInfo.FirstOrDefaultAsync();
        var targetUtc   = weddingInfo?.WeddingDate ?? DateTime.UtcNow.AddDays(90);

        var png = countdown.GenerateGuestMessagePng(user.FirstName, targetUtc);
        return File(png, "image/png");
    }
}
