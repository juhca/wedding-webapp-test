namespace WeddingApp_Test.Application.Interfaces;

public interface IPasswordHasher
{
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
}