using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string CreateJwtToken(User user)
    {
        var claims = new List<Claim>()
        {
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, user.Role.ToString()),
            new (ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetSection("TokenConfig:Token").Value!)
        );
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetSection("TokenConfig:Issuer").Value,
            audience: _configuration.GetSection("TokenConfig:Audience").Value,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );
        
        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        
        return token;
    }

    public RefreshToken CreateRefreshToken(User user)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        return refreshToken;
    }
}