namespace TicketService.Application.Models;

public sealed record PartnerCarTicketReviewData(
    string? CarBrand,
    string? CarModel,
    int? CarYear,
    string? LicensePlate,
    decimal? PriceHour,
    decimal? PriceDay,
    string? Email);
