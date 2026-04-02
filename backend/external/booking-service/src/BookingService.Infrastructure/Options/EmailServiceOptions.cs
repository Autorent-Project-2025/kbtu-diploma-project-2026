namespace BookingService.Infrastructure.Options;

public sealed class EmailServiceOptions
{
    public const string SectionName = "EmailService";

    public string BaseUrl { get; set; } = string.Empty;
}
