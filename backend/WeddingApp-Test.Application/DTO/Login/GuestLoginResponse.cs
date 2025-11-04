namespace WeddingApp_Test.Application.DTO;

public record GuestLoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresOn);