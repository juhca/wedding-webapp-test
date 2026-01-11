using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.DTO;

public record LoginResponse(string AccessToken, RefreshTokenDto RefreshToken, DateTime ExpiresOn);