namespace IdentityService.Application.Commands.ActivateUser;

public sealed record ActivateUserCommand(string ActivationToken, string Password);
