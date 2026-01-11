using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.DTO.Auth;

public record LoginResponseDto(string Token, RefreshTokenDto  RefreshToken);