namespace TicketService.Infrastructure.Options;

public sealed class ImageServiceOptions
{
    public const string SectionName = "ImageService";

    public string BaseUrl { get; init; } = string.Empty;
}
