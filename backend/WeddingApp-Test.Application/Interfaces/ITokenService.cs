using WeddingApp_Test.Application.DTO;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface ITokenService
{
    string CreateJwtToken(User user);
    RefreshToken CreateRefreshToken(User user);
}