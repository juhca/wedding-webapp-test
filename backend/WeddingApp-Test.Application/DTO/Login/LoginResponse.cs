namespace WeddingApp_Test.Application.DTO.Login;

public record LoginResponse(string AccessToken, RefreshTokenDto RefreshToken, DateTime ExpiresOn);