namespace BookingService.Infrastructure.Options;

public sealed class TicketServiceOptions
{
    public const string SectionName = "TicketService";

    public string BaseUrl { get; set; } = string.Empty;
}
