namespace WeddingApp_Test.Application.DTO;

public record RefreshTokenDto(
    string Token,
    DateTime Expires,
    DateTime Created
);