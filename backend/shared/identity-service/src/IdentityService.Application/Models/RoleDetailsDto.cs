namespace IdentityService.Application.Models;

public sealed record RoleDetailsDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<string> DirectPermissions,
    IReadOnlyCollection<RoleReferenceDto> ParentRoles);
