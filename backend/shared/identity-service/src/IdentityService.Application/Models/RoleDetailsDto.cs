namespace IdentityService.Application.Models;

public sealed record RoleDetailsDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Permissions);
