namespace AutoRent.Messaging.Contracts;

public sealed record PartnerCarProvisionRequested(
    Guid TicketId,
    string ProvisionRequestKey,
    Guid RelatedUserId,
    string CarBrand,
    string CarModel,
    int CarYear,
    string LicensePlate,
    string? Transmission,
    string? FuelType,
    int? Seats,
    int? Doors,
    string? BodyType,
    int? Horsepower,
    IReadOnlyCollection<string> SemanticTags,
    string OwnershipDocumentFileName,
    IReadOnlyCollection<PartnerCarProvisionRequestedImage> Images);

public sealed record PartnerCarProvisionRequestedImage(
    string ImageId,
    string ImageUrl);
