namespace TicketService.Application.Models;

public sealed record ProvisionPartnerCarRequest(
    Guid RelatedUserId,
    string CarBrand,
    string CarModel,
    string LicensePlate,
    string OwnershipDocumentFileName,
    IReadOnlyCollection<ProvisionPartnerCarImageRequest> Images);

public sealed record ProvisionPartnerCarImageRequest(
    string ImageId,
    string ImageUrl);
