using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;

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
            cancellationToken: cancellationToken);

        var roleDtos = roles
            .Select(role =>
            {
                var permissions = role.Permissions
                    .Select(permission => permission.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new RoleDetailsDto(role.Id, role.Name, permissions);
            })
            .ToArray();

        return new GetRolesResult(roleDtos);
    }
}
