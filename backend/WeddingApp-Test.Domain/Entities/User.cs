using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Domain.Entities;

public class User
{
	public Guid Id { get; set; }
	
	public string Name { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	
	// only admins have password-based login; guest will use access codes
	public string? PasswordHash { get; set; } = string.Empty;
	public string? AccessCode { get; set; } = string.Empty;
	
	public UserRole Role { get; set; }
	
}
