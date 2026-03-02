namespace IdentityService.Application.Commands.CreatePermission;

public sealed record CreatePermissionResult(Guid PermissionId, string Name, string Description);
