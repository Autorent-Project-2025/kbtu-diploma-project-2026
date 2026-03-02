namespace TicketService.Infrastructure.Options;

public sealed class EmailServiceOptions
{
    public const string SectionName = "EmailService";

    public string BaseUrl { get; init; } = string.Empty;
}
