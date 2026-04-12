namespace TicketService.Application.Models;

public sealed record ApplyPartnerCarApprovedUpdateRequest(
    int PartnerCarId,
    string LicensePlate,
    string? Color,
    int? RequestedStatus,
    bool? IsActive,
    IReadOnlyCollection<ApplyPartnerCarApprovedUpdateImageRequest> Images);

public sealed record ApplyPartnerCarApprovedUpdateImageRequest(
    string ImageId,
    string ImageUrl,
    string ImageType);
