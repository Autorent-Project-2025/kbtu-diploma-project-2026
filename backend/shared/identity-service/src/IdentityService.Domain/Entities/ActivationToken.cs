namespace IdentityService.Domain.Entities;

public class ActivationToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public User? User { get; private set; }

    private ActivationToken() { }

    public ActivationToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
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
            throw new ArgumentException("Activation token expiration must be later than creation time.", nameof(expiresAtUtc));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId;
        TokenHash = tokenHash.Trim();
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsActive(DateTime nowUtc)
    {
        return UsedAtUtc is null && ExpiresAtUtc > nowUtc;
    }

    public void MarkAsUsed(DateTime usedAtUtc)
    {
        if (UsedAtUtc is not null)
        {
            return;
        }

        UsedAtUtc = usedAtUtc;
    }
}
