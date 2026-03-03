namespace IdentityService.Application.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    IReadOnlyCollection<Guid>? PermissionIds,
    IReadOnlyCollection<Guid>? ParentRoleIds);
