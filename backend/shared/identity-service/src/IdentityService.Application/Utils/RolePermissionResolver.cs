using IdentityService.Domain.Entities;

namespace IdentityService.Application.Utils;

public static class RolePermissionResolver
{
    public static IReadOnlyDictionary<Guid, RoleGraphNode> BuildGraph(IEnumerable<Role> roles)
    {
        return roles
            .GroupBy(role => role.Id)
            .Select(group => group.First())
            .ToDictionary(
                role => role.Id,
                role => new RoleGraphNode(
                    role.Id,
                    role.Name,
                    role.ParentRoles
                        .Select(parentRole => parentRole.Id)
                        .Where(parentRoleId => parentRoleId != Guid.Empty)
                        .Distinct()
                        .ToArray(),
                    role.Permissions
                        .Select(permission => permission.Name)
                        .Where(permissionName => !string.IsNullOrWhiteSpace(permissionName))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()));
    }

    public static IReadOnlyCollection<string> ResolveEffectivePermissions(
        IEnumerable<Guid> roleIds,
        IReadOnlyDictionary<Guid, RoleGraphNode> roleGraph)
    {
        var permissions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitedRoleIds = new HashSet<Guid>();
        var pendingRoleIds = new Stack<Guid>(
            roleIds
                .Where(roleId => roleId != Guid.Empty)
                .Distinct());

        while (pendingRoleIds.Count > 0)
        {
            var currentRoleId = pendingRoleIds.Pop();
            if (!visitedRoleIds.Add(currentRoleId))
            {
                continue;
            }

            if (!roleGraph.TryGetValue(currentRoleId, out var roleNode))
            {
                continue;
            }

            foreach (var permissionName in roleNode.DirectPermissionNames)
            {
                permissions.Add(permissionName);
            }

            foreach (var parentRoleId in roleNode.ParentRoleIds)
            {
                pendingRoleIds.Push(parentRoleId);
            }
        }

        return permissions.ToArray();
    }

    public static IReadOnlyCollection<string> ResolveEffectivePermissionsForRole(
        Guid roleId,
        IReadOnlyDictionary<Guid, RoleGraphNode> roleGraph)
    {
        return ResolveEffectivePermissions([roleId], roleGraph);
    }

    public static IReadOnlyCollection<Guid> ResolveAncestorRoleIds(
        Guid roleId,
        IReadOnlyDictionary<Guid, RoleGraphNode> roleGraph)
    {
        if (!roleGraph.TryGetValue(roleId, out var roleNode))
        {
            return [];
        }

        var ancestors = new HashSet<Guid>();
        var pendingRoleIds = new Stack<Guid>(roleNode.ParentRoleIds);

        while (pendingRoleIds.Count > 0)
        {
            var parentRoleId = pendingRoleIds.Pop();
            if (!ancestors.Add(parentRoleId))
            {
                continue;
            }

            if (!roleGraph.TryGetValue(parentRoleId, out var parentNode))
            {
                continue;
            }

            foreach (var upperParentRoleId in parentNode.ParentRoleIds)
            {
                pendingRoleIds.Push(upperParentRoleId);
            }
        }

        return ancestors.ToArray();
    }
}

public sealed record RoleGraphNode(
    Guid Id,
    string Name,
    IReadOnlyCollection<Guid> ParentRoleIds,
    IReadOnlyCollection<string> DirectPermissionNames);
