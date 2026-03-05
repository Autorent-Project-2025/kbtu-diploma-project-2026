namespace TicketService.Application.Models;

public sealed record ProvisionPartnerCarRequest(
    Guid RelatedUserId,
    string CarBrand,
    string CarModel,
    int CarYear,
    string LicensePlate,
    decimal PriceHour,
    decimal PriceDay,
    string OwnershipDocumentFileName,
    IReadOnlyCollection<ProvisionPartnerCarImageRequest> Images);

public sealed record ProvisionPartnerCarImageRequest(
    string ImageId,
    string ImageUrl);
