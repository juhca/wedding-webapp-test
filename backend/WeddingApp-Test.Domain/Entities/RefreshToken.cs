namespace WeddingApp_Test.Domain.Entities;

public record RefreshToken(string Token, DateTime Created, DateTime ExpiryTime);