namespace IdentityService.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    private Role() { }

    public Role(Guid id, string name)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    public void AddPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (Permissions.Any(existingPermission => existingPermission.Id == permission.Id))
        {
            return;
        }

        Permissions.Add(permission);
    }

    public bool HasPermission(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
        {
            return false;
        }

        return Permissions.Any(permission =>
            permission.Name.Equals(permissionName.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
