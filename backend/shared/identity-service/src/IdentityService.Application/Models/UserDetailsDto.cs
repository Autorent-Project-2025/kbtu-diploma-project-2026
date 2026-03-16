namespace IdentityService.Application.Models;

public sealed record UserDetailsDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    string SubjectType,
    string ActorType,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
