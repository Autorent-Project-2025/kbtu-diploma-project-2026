namespace TicketService.Infrastructure.Options;

public sealed class CarServiceOptions
{
    public const string SectionName = "CarService";

    public string BaseUrl { get; init; } = string.Empty;
    public string InternalApiKey { get; init; } = string.Empty;
}
