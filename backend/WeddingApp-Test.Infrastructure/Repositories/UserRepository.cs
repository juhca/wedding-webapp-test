using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeddingApp_Test.Application.Common.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
	public async Task AddAsync(User user)
	{
		context.Users.Add(user);
		await context.SaveChangesAsync();
	}

	public Task<bool> CheckIfUserExists(User user)
	{
		return context.Users.AnyAsync(u => 
			u.FirstName.Equals(user.FirstName, StringComparison.InvariantCultureIgnoreCase) 
			&& u.LastName.Equals(user.LastName, StringComparison.InvariantCultureIgnoreCase));
	}

	public async Task<User?> GetByEmailAsync(string email)
	{
		return await context.Users
			.Include(u => u.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Email.Equals(email));
	}

	public async Task<User?> GetByAccessCode(string accessCode)
	{
		return await context.Users
			.Include(u => u.RefreshTokens)
			.FirstOrDefaultAsync(x => x.AccessCode == accessCode);
	}

	public async Task<IEnumerable<User>> GetAllAsync()
	{
		return await context.Users.ToListAsync();
	}

	public async Task<User?> GetByIdAsync(Guid id)
	{
		return await context.Users.FindAsync(id);
	}
	
	public async Task AddRefreshTokenAsync(User user, RefreshToken refreshToken)
	{
		refreshToken.UserId =  user.Id;
		context.RefreshTokens.Add(refreshToken);
		await context.SaveChangesAsync();
	}
	
	public async Task RemoveExpiredTokens(Guid userId)
	{
		// Cannot use GetByIdAsync here because it uses FindAsync (which doesn't Include tokens).
		// We must manually query with Include to ensure the RefreshTokens list is populated.
		var user = await context.Users
			.Include(u => u.RefreshTokens)
			.FirstOrDefaultAsync(u => u.Id == userId);
		
		if (user is null)
		{
			return;
		}
		
		user.RefreshTokens.RemoveAll(rt => rt.Expires < DateTime.UtcNow);
		await context.SaveChangesAsync();
	}
}
