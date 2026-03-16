namespace IdentityService.Application.Commands.CreateUser;

public sealed record CreateUserResult(
    Guid UserId,
    string Username,
    string Email,
    string SubjectType,
    string ActorType,
    IReadOnlyCollection<string> Roles);
