namespace IdentityService.Api.Contracts.Roles;

public sealed class AddPermissionToRoleRequest
{
    public Guid PermissionId { get; init; }
}
