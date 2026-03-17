namespace ClientService.Api.Contracts.Internal;

public sealed class ProvisionClientRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string? IdentityDocumentFileName { get; init; }
    public string? DriverLicenseFileName { get; init; }
    public Guid RelatedUserId { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? ProvisionRequestKey { get; init; }
}
