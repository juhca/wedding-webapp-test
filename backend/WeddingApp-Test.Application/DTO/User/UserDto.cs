using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.User;

public record UserDto(string FirstName, string LastName, string? Email, byte[]? PasswordHash, string? AccessCode, UserRole Role, int? MaxCompanions);