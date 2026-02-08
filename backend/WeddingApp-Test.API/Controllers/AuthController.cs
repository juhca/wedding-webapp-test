using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    
    [HttpPost]
    [Route("AdminLogin")]
    public async Task<ActionResult<LoginResponse>> AdminLogin(AdminLoginRequest loginRequest)
    {
        var adminLoginResult = await authService.AdminLogin(loginRequest);

        if (adminLoginResult is null)
        {
            return Unauthorized("Invalid username or password");
        }
        
        return Ok(adminLoginResult);
    }
	
    [HttpPost]
    [Route("GuestLogin")]
    public async Task<ActionResult<LoginResponse>> GuestLogin(GuestLoginRequest loginRequest)
    {
        var result = await authService.GuestLogin(loginRequest);

        if (result is null)
        {
            return Unauthorized("Invalid Access Code");
        }

        return Ok(result);
    }

    [HttpPost("Refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh(RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request);

        if (result is null)
        {
            return Unauthorized("Invalid or expired refresh token");
        }

        return Ok(result);
    }

    [HttpPost("Revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request)
    {
        var success = await authService.RevokeTokenAsync(request.RefreshToken);

        if (!success)
        {
            return BadRequest("Token not found or already revoked");
        }

        return Ok(new { message = "Token revoked successfully" });
    }
}