using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<GetPermissionsResult> Handle(
        GetPermissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.ListAsync(cancellationToken);

        var permissionDtos = permissions
            .Select(permission => new PermissionDetailsDto(
                permission.Id,
                permission.Name,
                permission.Description))
            .ToArray();

        return new GetPermissionsResult(permissionDtos);
    }
}
