namespace WeddingApp_Test.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}