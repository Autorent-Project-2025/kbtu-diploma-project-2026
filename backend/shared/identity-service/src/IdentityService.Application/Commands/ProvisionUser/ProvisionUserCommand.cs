namespace IdentityService.Application.Commands.ProvisionUser;

public sealed record ProvisionUserCommand(string FullName, string Email, DateOnly BirthDate);
