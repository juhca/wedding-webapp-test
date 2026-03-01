using System.ComponentModel.DataAnnotations;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Domain.Entities;

public class User
{
	public Guid Id { get; set; }
	
	[Required, MaxLength(50)]
	public string FirstName { get; set; } = string.Empty;
	[Required, MaxLength(50)]
	public string LastName { get; set; } = string.Empty;
	[EmailAddress, MaxLength(255)]
	public string Email { get; set; } = string.Empty;
	
	
	// Authentication
	// only admins have password-based login; guest will use access codes
	public string? AccessCode { get; set; } = string.Empty;
	public byte[]? PasswordHash { get; set; }
	public byte[]? PasswordSalt { get; set; }
	public List<RefreshToken> RefreshTokens { get; set; } = [];
	
	// Authorization
	public UserRole Role
	{
		get; set; 
		
	}
	
	/// <summary>
	/// Maximum number of companions this guest can bring (default 1)
	/// 0 = no companions allowed
	/// null = unlimited (admin only)
	/// </summary>
	public int? MaxCompanions { get; set; } = 1;
}
