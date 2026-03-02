namespace IdentityService.Application.Commands.RegisterUser;

public sealed record RegisterUserResult(Guid UserId, string Username, string Email);
