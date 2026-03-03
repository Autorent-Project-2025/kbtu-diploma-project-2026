namespace IdentityService.Application.Commands.AssignParentRoleToRole;

public sealed record AssignParentRoleToRoleCommand(Guid RoleId, Guid ParentRoleId);
