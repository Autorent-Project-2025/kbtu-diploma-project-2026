namespace BookingService.Infrastructure.Options;

public sealed class IdentityServiceOptions
{
    public const string SectionName = "IdentityService";

    public string BaseUrl { get; set; } = string.Empty;
    public string InternalApiKey { get; set; } = string.Empty;
}
