namespace TicketService.Api.Contracts.Tickets;

public sealed class PartnerCarTicketReviewDataRequest
{
    public string? CarBrand { get; init; }
    public string? CarModel { get; init; }
    public int? CarYear { get; init; }
    public string? LicensePlate { get; init; }
    public decimal? PriceHour { get; init; }
    public decimal? PriceDay { get; init; }
    public string? Email { get; init; }
}
