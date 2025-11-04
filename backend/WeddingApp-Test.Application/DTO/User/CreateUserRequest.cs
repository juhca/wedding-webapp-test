using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.DTO.User;

public record CreateUserRequest(string FirstName, string LastName, string? Email, string? Password, UserRole Role);