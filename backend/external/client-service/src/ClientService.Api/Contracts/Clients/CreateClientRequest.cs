using System.ComponentModel.DataAnnotations;

namespace ClientService.Api.Contracts.Clients;

public sealed class CreateClientRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    [MaxLength(255)]
    public string? IdentityDocumentFileName { get; set; }

    [MaxLength(255)]
    public string? DriverLicenseFileName { get; set; }

    [Required]
    [MaxLength(64)]
    public string RelatedUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? AvatarUrl { get; set; }
}
