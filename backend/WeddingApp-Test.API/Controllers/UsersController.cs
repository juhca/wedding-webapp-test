using Microsoft.AspNetCore.Mvc;
using WeddingApp_Test.Application.Common.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await userRepository.GetAllAsync());
	}

	[HttpGet("find")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var user = await userRepository.GetByIdAsync(id);	

		return Ok(user);
	}

	[HttpPost]
	public async Task<IActionResult> Add(User user)
	{
		await userRepository.AddAsync(user);

		return Ok(user);
	}
}
