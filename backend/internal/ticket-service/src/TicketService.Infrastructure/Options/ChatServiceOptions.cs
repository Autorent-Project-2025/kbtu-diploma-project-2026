namespace TicketService.Infrastructure.Options;

public sealed class ChatServiceOptions
{
    public const string SectionName = "ChatService";

    public string BaseUrl { get; init; } = string.Empty;
    public string InternalApiKey { get; init; } = string.Empty;
}
