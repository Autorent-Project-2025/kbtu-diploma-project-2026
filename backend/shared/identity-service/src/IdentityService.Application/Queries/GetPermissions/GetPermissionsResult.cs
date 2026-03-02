using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetPermissions;

public sealed record GetPermissionsResult(IReadOnlyCollection<PermissionDetailsDto> Permissions);
