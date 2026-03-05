namespace TicketService.Application.Models;

public sealed record PartnerCarTicketReviewData(
    string? CarBrand,
    string? CarModel,
    int? CarYear,
    string? LicensePlate,
    string? Email);
