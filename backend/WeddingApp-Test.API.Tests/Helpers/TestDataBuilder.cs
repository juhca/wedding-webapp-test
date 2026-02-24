using Microsoft.AspNetCore.Identity;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Services;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.API.Tests.Helpers;

/// <summary>
/// Helper class to create test data for users.
/// This makes it easy to set up different test scenarios.
/// </summary>
public class TestDataBuilder
{
    private static readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    /// <summary>
    /// Creates an admin user with email/password authentication
    /// </summary>
    public static User CreateAdminUser(string email, string password)
	{
		_passwordHasher.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

		return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            AccessCode = null,
            Role = UserRole.Admin,
            RefreshTokens = []
        };
    }
    
    /// <summary>
    /// Creates a guest user with access code authentication
    /// </summary>
    public static User CreateGuestUser(string accessCode, UserRole role = UserRole.FullExperience)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Guest",
            LastName = "User",
            Email = $"guest_{Guid.NewGuid()}@example.com",
            PasswordHash = null,
            PasswordSalt = null,
            AccessCode = accessCode,
            Role = role,
            RefreshTokens = []
        };
    }
    
    /// <summary>
    /// Creates a simple password hash/salt pair for testing
    /// In real tests, you'd use your actual PasswordHasher service
    /// </summary>
    public static (byte[] hash, byte[] salt) CreatePasswordHashAndSalt(string password)
    {
        // Simple hash for testing
        var hash = System.Text.Encoding.UTF8.GetBytes($"hash_{password}");
        var salt = System.Text.Encoding.UTF8.GetBytes($"salt_{password}");
        
        return (hash, salt);
    }
}