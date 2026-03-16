using IdentityService.Domain.Constants;

namespace IdentityService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string SubjectType { get; private set; } = SubjectTypeConstants.User;
    public string ActorType { get; private set; } = ActorTypeConstants.Client;

    public ICollection<Role> Roles { get; private set; } = new List<Role>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public User(
        Guid id,
        string username,
        string email,
        string passwordHash,
        bool isActive = true,
        string subjectType = SubjectTypeConstants.User,
        string actorType = ActorTypeConstants.Client)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetUsername(username);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        IsActive = isActive;
        SetSubjectType(subjectType);
        SetActorType(actorType);
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

    public void SetSubjectType(string subjectType)
    {
        SubjectType = NormalizeKnownType(
            subjectType,
            nameof(subjectType),
            "Subject type",
            SubjectTypeConstants.All);
    }

    public void SetActorType(string actorType)
    {
        ActorType = NormalizeKnownType(
            actorType,
            nameof(actorType),
            "Actor type",
            ActorTypeConstants.All);
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

    public void RemoveRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            return;
        }

        var role = Roles.FirstOrDefault(existingRole => existingRole.Id == roleId);
        if (role is null)
        {
            return;
        }

        Roles.Remove(role);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool HasPermission(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
        {
            return false;
        }

        return Roles.Any(role => role.HasPermission(permissionName));
    }

    public bool IsSubjectType(string subjectType)
    {
        if (string.IsNullOrWhiteSpace(subjectType))
        {
            return false;
        }

        return string.Equals(SubjectType, subjectType.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public bool IsActorType(string actorType)
    {
        if (string.IsNullOrWhiteSpace(actorType))
        {
            return false;
        }

        return string.Equals(ActorType, actorType.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeKnownType(
        string value,
        string parameterName,
        string label,
        HashSet<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{label} cannot be empty.", parameterName);
        }

        var normalizedValue = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalizedValue))
        {
            throw new ArgumentException(
                $"{label} '{normalizedValue}' is not supported.",
                parameterName);
        }

        return normalizedValue;
    }
}
