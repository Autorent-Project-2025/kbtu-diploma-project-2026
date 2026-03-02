namespace IdentityService.Api.Contracts.Users;

public sealed class AssignRoleToUserRequest
{
    public Guid RoleId { get; init; }
}
