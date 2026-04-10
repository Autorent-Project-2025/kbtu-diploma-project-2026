namespace TicketService.Api.Contracts.Tickets;

public sealed class PartnerCarTicketReviewDataRequest
{
    public string? CarBrand { get; init; }
    public string? CarModel { get; init; }
    public int? CarYear { get; init; }
    public string? LicensePlate { get; init; }
    public string? Transmission { get; init; }
    public string? FuelType { get; init; }
    public int? Seats { get; init; }
    public int? Doors { get; init; }
    public string? BodyType { get; init; }
    public int? Horsepower { get; init; }
    public List<string>? ConfirmedTags { get; init; }
}
