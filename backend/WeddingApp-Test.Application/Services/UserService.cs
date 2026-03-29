using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using WeddingApp_Test.Application.DTO.User;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    
    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }
    
    public async Task<UserDto?> CreateUserAsync(CreateUserRequest user)
    {
        // Add new user
        var newUser = new User()
        {
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Id = Guid.NewGuid()
        };

        var allowUserDuplicates = _configuration.GetSection("User:AllowDuplicates").Value!;
        if (allowUserDuplicates.ToLower().Equals("false"))
        {
            if (await _userRepository.CheckIfUserExists(newUser))
            {
                return null;
            }
        }
        
        // If it has password it must be admin
        if (!string.IsNullOrEmpty(user.Password))
        {
            _passwordHasher.CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            newUser.PasswordHash = passwordHash;
            newUser.PasswordSalt = passwordSalt;
            newUser.Role = UserRole.Admin;
        }
        else
        {
            // if it doesn't have a password generate an access code
            // TODO(TOMAS) generate good access code
            newUser.AccessCode = await GenerateAccessCode(6);
        }
        
        await _userRepository.AddAsync(newUser);

        return new UserDto(newUser.FirstName, newUser.LastName,newUser.Email, newUser.PasswordHash, newUser.AccessCode, newUser.Role, newUser.MaxCompanions);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(user => 
            new UserDto(user.FirstName, user.LastName, user.Email, user.PasswordHash, user.AccessCode, user.Role, user.MaxCompanions))
            .ToList();
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new UserDto(user.FirstName, user.LastName, user.Email, user.PasswordHash, user.AccessCode, user.Role, user.MaxCompanions);
    }

    public async Task<UserDto?> UpdateEmailAsync(Guid userId, UpdateUserEmailRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null && existing.Id != userId)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
        }

        user.Email = request.Email;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new UserDto(user.FirstName, user.LastName, user.Email, user.PasswordHash, user.AccessCode, user.Role, user.MaxCompanions);
    }

    private async Task<string> GenerateAccessCode(int length = 6)
    { 
        var chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        
        if (length <= 0)
        {
            throw new ArgumentException("Length must be greater than 0.", nameof(length));
        }

        var result = new StringBuilder();
        var randomBytes = new byte[length];
        
        while (true)
        {
            // Fill the byte array with cryptographically secure random numbers.
            RandomNumberGenerator.Fill(randomBytes);
        
            // Convert each random byte to a character from our allowed set.
            foreach (var byteValue in randomBytes)
            {
                // The modulo operator ensures the index is within the bounds of the character array.
                result.Append(chars[byteValue % chars.Length]);
            }
            
            // Check if accessCode truly unique
            var user = await _userRepository.GetByAccessCode(result.ToString());
            if (user == null)
            {
                break;    
            }
        }
        
        return result.ToString();
    }
}