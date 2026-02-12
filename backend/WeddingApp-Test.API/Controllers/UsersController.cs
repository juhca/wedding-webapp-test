using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO;
using WeddingApp_Test.Application.DTO.User;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> AddUser(CreateUserRequest user)
	{
		var newUser = await userService.CreateUserAsync(user);
		if (newUser is null)
		{
			return BadRequest("User already exists.");
		}
		
		return Ok(newUser);
	}
	
	[HttpGet]
	[Authorize(Roles = nameof(UserRole.Admin))]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await userService.GetAllUsersAsync());
	}
}
