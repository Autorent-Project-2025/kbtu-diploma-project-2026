namespace IdentityService.Api.Contracts.Roles;

public sealed class CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
}
