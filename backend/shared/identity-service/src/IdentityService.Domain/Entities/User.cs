namespace IdentityService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public ICollection<Role> Roles { get; private set; } = new List<Role>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public User(Guid id, string username, string email, string passwordHash)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetUsername(username);
        SetEmail(email);
        SetPasswordHash(passwordHash);
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    public void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty.", nameof(username));
        }

        Username = username.Trim();
    }

    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (Roles.Any(existingRole => existingRole.Id == role.Id))
        {
            return;
        }

        Roles.Add(role);
    }

    public bool HasPermission(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
        {
            return false;
        }

        return Roles.Any(role => role.HasPermission(permissionName));
    }
}
