namespace IdentityService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid id, Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("Refresh token expiration time is invalid.", nameof(expiresAtUtc));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsActive(DateTime utcNow)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > utcNow;
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;
    }
}
