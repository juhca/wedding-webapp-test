using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.DTO.User;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
	[HttpPost]
	[Route("AddUser")]
	[Authorize(Roles = nameof(UserRole.Admin))]
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
	[Route("GetAll")]
	[Authorize(Roles = nameof(UserRole.Admin))]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await userService.GetAllUsersAsync());
	}

	[HttpPatch("{id}/email")]
	[Authorize(Roles = nameof(UserRole.Admin))]
	[ProducesResponseType(typeof(UserDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(409)]
	public async Task<IActionResult> UpdateEmail(Guid id, [FromBody] UpdateUserEmailRequest request)
	{
		try
		{
			// TODO(TOMAS): send the user a confirmation email of changed email
			// TODO(TOMAS): email address must be confirmed by clicking the link on the email?
			var updatedUser = await userService.UpdateEmailAsync(id, request);
			if (updatedUser is null)
			{
				return NotFound($"User with ID {id} not found.");
			}
			
			return Ok(updatedUser);
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(ex.Message);
		}
	}
}
