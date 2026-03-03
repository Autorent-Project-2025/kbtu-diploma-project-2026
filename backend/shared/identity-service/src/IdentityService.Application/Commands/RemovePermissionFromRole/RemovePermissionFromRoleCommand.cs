namespace IdentityService.Application.Commands.RemovePermissionFromRole;

public sealed record RemovePermissionFromRoleCommand(Guid RoleId, Guid PermissionId);
