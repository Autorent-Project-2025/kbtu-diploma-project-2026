namespace IdentityService.Application.Commands.RemoveParentRoleFromRole;

public sealed record RemoveParentRoleFromRoleCommand(Guid RoleId, Guid ParentRoleId);
