namespace WeddingApp_Test.Application.DTO.Login;

public record RefreshTokenDto(
    string Token,
    DateTime Expires,
    DateTime Created
);