namespace IdentityService.Application.Commands.ActivateUser;

public sealed record ActivateUserResult(Guid UserId, string Email);
