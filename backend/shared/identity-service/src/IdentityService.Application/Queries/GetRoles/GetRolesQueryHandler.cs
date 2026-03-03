using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;

namespace IdentityService.Application.Queries.GetRoles;

public sealed class GetRolesQueryHandler
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<GetRolesResult> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.ListAsync(
            includePermissions: true,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        var roleGraph = RolePermissionResolver.BuildGraph(roles);

        var roleDtos = roles
            .Select(role =>
            {
                var directPermissions = role.Permissions
                    .Select(permission => permission.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var effectivePermissions = RolePermissionResolver.ResolveEffectivePermissionsForRole(
                    role.Id,
                    roleGraph);

                var parentRoles = role.ParentRoles
                    .Select(parentRole => new RoleReferenceDto(parentRole.Id, parentRole.Name))
                    .OrderBy(parentRole => parentRole.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new RoleDetailsDto(
                    role.Id,
                    role.Name,
                    effectivePermissions,
                    directPermissions,
                    parentRoles);
            })
            .ToArray();

        return new GetRolesResult(roleDtos);
    }
}
