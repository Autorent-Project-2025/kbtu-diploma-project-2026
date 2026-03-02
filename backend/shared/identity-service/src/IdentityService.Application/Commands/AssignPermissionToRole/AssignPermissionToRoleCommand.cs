namespace IdentityService.Application.Commands.AssignPermissionToRole;

public sealed record AssignPermissionToRoleCommand(Guid RoleId, Guid PermissionId);
