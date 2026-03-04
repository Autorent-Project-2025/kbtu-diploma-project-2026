namespace TicketService.Application.Models;

public sealed record PartnerCarTicketReviewData(
    string? CarBrand,
    string? CarModel,
    string? LicensePlate,
    string? Email);
