using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO;
using WeddingApp_Test.Application.DTO.User;
using WeddingApp_Test.Application.Interfaces;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
	[HttpPost]
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
	public async Task<IActionResult> GetAll()
	{
		return Ok(await userService.GetAllUsersAsync());
	}
}
