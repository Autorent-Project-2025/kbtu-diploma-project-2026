namespace IdentityService.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Username, string Email, string Password);
