namespace IdentityService.Application.Commands.AssignRoleToUser;

public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId);
