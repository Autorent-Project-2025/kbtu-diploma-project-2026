namespace PartnerService.Application.DTOs;

public sealed class PartnerCreateDto
{
    public string OwnerFirstName { get; set; } = string.Empty;
    public string OwnerLastName { get; set; } = string.Empty;
    public string? ContractFileName { get; set; }
    public string OwnerIdentityFileName { get; set; } = string.Empty;
    public DateOnly RegistrationDate { get; set; }
    public DateOnly PartnershipEndDate { get; set; }
    public string RelatedUserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProvisionRequestKey { get; set; }
}
