namespace IdentityService.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();
    public ICollection<Role> ParentRoles { get; private set; } = new List<Role>();
    public ICollection<Role> ChildRoles { get; private set; } = new List<Role>();

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

    public void RemovePermission(Guid permissionId)
    {
        if (permissionId == Guid.Empty)
        {
            return;
        }

        var permission = Permissions.FirstOrDefault(existingPermission => existingPermission.Id == permissionId);
        if (permission is null)
        {
            return;
        }

        Permissions.Remove(permission);
    }

    public void AddParentRole(Role parentRole)
    {
        ArgumentNullException.ThrowIfNull(parentRole);

        if (parentRole.Id == Id)
        {
            throw new ArgumentException("A role cannot inherit from itself.", nameof(parentRole));
        }

        if (ParentRoles.Any(existingParent => existingParent.Id == parentRole.Id))
        {
            return;
        }

        ParentRoles.Add(parentRole);
    }

    public void RemoveParentRole(Guid parentRoleId)
    {
        if (parentRoleId == Guid.Empty)
        {
            return;
        }

        var parentRole = ParentRoles.FirstOrDefault(existingParent => existingParent.Id == parentRoleId);
        if (parentRole is null)
        {
            return;
        }

        ParentRoles.Remove(parentRole);
    }

    public bool HasPermission(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
        {
            return false;
        }

        var visitedRoleIds = new HashSet<Guid>();
        return HasPermissionInternal(permissionName.Trim(), visitedRoleIds);
    }

    private bool HasPermissionInternal(string permissionName, HashSet<Guid> visitedRoleIds)
    {
        if (!visitedRoleIds.Add(Id))
        {
            return false;
        }

        if (Permissions.Any(permission =>
            permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return ParentRoles.Any(parentRole => parentRole.HasPermissionInternal(permissionName, visitedRoleIds));
    }
}
