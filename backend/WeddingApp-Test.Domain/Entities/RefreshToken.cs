namespace WeddingApp_Test.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }

    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }

    // Revocation tracking
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    // Computed property - not stored in DB
    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;

    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}