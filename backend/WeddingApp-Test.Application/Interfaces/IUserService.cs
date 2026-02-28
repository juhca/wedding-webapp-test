using WeddingApp_Test.Application.DTO.User;

namespace WeddingApp_Test.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> CreateUserAsync(CreateUserRequest user);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserAsync(Guid userId);
}