namespace IdentityService.Application.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    IReadOnlyCollection<string>? RoleNames,
    string? SubjectType,
    string? ActorType);
