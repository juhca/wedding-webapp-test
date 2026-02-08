using WeddingApp_Test.Application.Common.Interfaces;
using WeddingApp_Test.Application.DTO;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class AuthService : IAuthService
{
    private readonly IPasswordHasher _passwordHasher;
    //private readonly IConfiguration _configuration;
    private readonly IUserRepository  _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenService tokenService)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _tokenService = tokenService;
    }
    
    public async Task<LoginResponseDto?> GuestLogin(GuestLoginRequest loginRequest)
    {
        var user = await _userRepository.GetByAccessCode(loginRequest.AccessCode);
        if (user is null)
        {
            return null; // code does not exist
        }
        
        // Housekeeping ~ remove expired refresh tokens
        await _userRepository.RemoveExpiredTokens(user.Id);
        
        // Create tokens 
        var accessToken = _tokenService.CreateJwtToken(user);
        var refreshToken = _tokenService.CreateRefreshToken(user);
        
        // Set the refresh token
        await _userRepository.AddRefreshTokenAsync(user, refreshToken);
        
        return new LoginResponseDto(accessToken, new RefreshTokenDto(refreshToken.Token, refreshToken.Expires, refreshToken.Created));
    }

    public async Task<LoginResponseDto?> AdminLogin(AdminLoginRequest loginRequest)
    {
        var user = await _userRepository.GetByEmailAsync(loginRequest.Email);
        
        // Check if user is null, if not check if password config present
        if (user?.PasswordHash is null || user.PasswordSalt is null)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        if (user.Role is not UserRole.Admin)
        {
            return null;
        }
        
        // Remove old tokens
        await _userRepository.RemoveExpiredTokens(user.Id);
        
        // User is verified now create token
        var token = _tokenService.CreateJwtToken(user);
        
        // JWT token created, now create refresh token
        var refreshToken = _tokenService.CreateRefreshToken(user);
        await _userRepository.AddRefreshTokenAsync(user, refreshToken);
        
        return new LoginResponseDto(token, new RefreshTokenDto(refreshToken.Token, refreshToken.Expires, refreshToken.Created));
    }
}