namespace TicketService.Infrastructure.Options;

public sealed class ClientServiceOptions
{
    public const string SectionName = "ClientService";

    public string BaseUrl { get; init; } = string.Empty;
    public string InternalApiKey { get; init; } = string.Empty;
}
