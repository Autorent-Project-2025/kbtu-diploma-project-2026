using System.ComponentModel.DataAnnotations;

namespace PartnerService.Api.Contracts.Partners;

public sealed class UpdatePartnerRequest
{
    [Required]
    [MaxLength(100)]
    public string OwnerFirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string OwnerLastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ContractFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OwnerIdentityFileName { get; set; } = string.Empty;

    public DateOnly RegistrationDate { get; set; }

    public DateOnly PartnershipEndDate { get; set; }

    [Required]
    [MaxLength(64)]
    public string RelatedUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;
}
