namespace TicketService.Application.Models;

public sealed record PartnerCarTicketReviewData(
    string? CarBrand,
    string? CarModel,
    int? CarYear,
    string? LicensePlate,
    string? Transmission,
    string? FuelType,
    int? Seats,
    int? Doors,
    string? BodyType,
    int? Horsepower,
    IReadOnlyCollection<string>? ConfirmedTags);
