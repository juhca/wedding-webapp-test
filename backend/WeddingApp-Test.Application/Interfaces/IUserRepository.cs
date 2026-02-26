using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IUserRepository
{
	Task<User?> GetByIdAsync(Guid id);
	Task<User?> GetByEmailAsync(string email);
	Task<User?> GetByAccessCode(string accessCode);
	Task<IEnumerable<User>> GetAllAsync();
	Task AddAsync(User user);
	Task<bool> CheckIfUserExists(User user);
	Task RemoveExpiredTokens(Guid userId);
	Task AddRefreshTokenAsync(User user, RefreshToken refreshToken);
	Task<RefreshToken?> GetRefreshTokenAsync(string token);
	Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedByToken = null);
	Task RevokeAllUserTokensAsync(Guid userId);
}
