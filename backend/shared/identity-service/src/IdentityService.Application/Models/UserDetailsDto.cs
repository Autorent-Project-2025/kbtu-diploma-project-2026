namespace IdentityService.Application.Models;

public sealed record UserDetailsDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
