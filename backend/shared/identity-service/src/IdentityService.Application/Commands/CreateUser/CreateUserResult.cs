namespace IdentityService.Application.Commands.CreateUser;

public sealed record CreateUserResult(Guid UserId, string Username, string Email, IReadOnlyCollection<string> Roles);
