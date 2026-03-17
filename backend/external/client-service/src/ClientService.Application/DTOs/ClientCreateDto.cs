namespace ClientService.Application.DTOs;

public sealed class ClientCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? IdentityDocumentFileName { get; set; }
    public string? DriverLicenseFileName { get; set; }
    public string RelatedUserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? ProvisionRequestKey { get; set; }
}
