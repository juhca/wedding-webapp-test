using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	
	public UserRole Role { get; set; }
	
}
