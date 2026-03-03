namespace IdentityService.Api.Contracts.Roles;

public sealed class AddParentRoleToRoleRequest
{
    public Guid ParentRoleId { get; init; }
}
