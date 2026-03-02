namespace IdentityService.Application.Commands.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId);
