namespace IdentityService.Api.Contracts.Permissions;

public sealed class CreatePermissionRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
