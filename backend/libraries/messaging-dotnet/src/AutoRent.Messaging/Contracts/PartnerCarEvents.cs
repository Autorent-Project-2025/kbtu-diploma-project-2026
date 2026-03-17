namespace AutoRent.Messaging.Contracts;

public sealed record PartnerCarProvisionRequested(
    Guid TicketId,
    string ProvisionRequestKey,
    Guid RelatedUserId,
    string CarBrand,
    string CarModel,
    int CarYear,
    string LicensePlate,
    decimal PriceHour,
    decimal PriceDay,
    string OwnershipDocumentFileName,
    IReadOnlyCollection<PartnerCarProvisionRequestedImage> Images);

public sealed record PartnerCarProvisionRequestedImage(
    string ImageId,
    string ImageUrl);
