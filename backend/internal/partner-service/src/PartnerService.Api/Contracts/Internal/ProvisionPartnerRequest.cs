namespace PartnerService.Api.Contracts.Internal;

public sealed class ProvisionPartnerRequest
{
    public string OwnerFirstName { get; init; } = string.Empty;
    public string OwnerLastName { get; init; } = string.Empty;
    public string? ContractFileName { get; init; }
    public string OwnerIdentityFileName { get; init; } = string.Empty;
    public DateOnly RegistrationDate { get; init; }
    public DateOnly PartnershipEndDate { get; init; }
    public Guid RelatedUserId { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? ProvisionRequestKey { get; init; }
}
