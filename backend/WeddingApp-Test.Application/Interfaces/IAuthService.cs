using WeddingApp_Test.Application.DTO;
using WeddingApp_Test.Application.DTO.Auth;
using WeddingApp_Test.Application.DTO.Login;

namespace WeddingApp_Test.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> GuestLogin(GuestLoginRequest loginRequest);
    Task<LoginResponseDto?> AdminLogin(AdminLoginRequest loginRequest);
}