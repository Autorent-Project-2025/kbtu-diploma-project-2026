namespace IdentityService.Api.Contracts.Roles;

public sealed class CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyCollection<Guid>? PermissionIds { get; init; }
    public IReadOnlyCollection<Guid>? ParentRoleIds { get; init; }
}
