namespace TicketService.Application.Models;

public sealed record ProvisionPartnerProfileRequest(
    string OwnerFirstName,
    string OwnerLastName,
    string? ContractFileName,
    string OwnerIdentityFileName,
    DateOnly RegistrationDate,
    DateOnly PartnershipEndDate,
    Guid RelatedUserId,
    string PhoneNumber);
