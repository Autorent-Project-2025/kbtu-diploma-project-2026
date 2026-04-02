namespace BookingService.Infrastructure.Options;

public sealed class ClientServiceOptions
{
    public const string SectionName = "ClientService";

    public string BaseUrl { get; set; } = string.Empty;
    public string InternalApiKey { get; set; } = string.Empty;
}
