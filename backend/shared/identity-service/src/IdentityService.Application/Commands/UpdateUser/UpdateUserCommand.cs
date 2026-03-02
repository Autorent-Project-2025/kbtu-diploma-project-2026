namespace IdentityService.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string Username, string Email);
