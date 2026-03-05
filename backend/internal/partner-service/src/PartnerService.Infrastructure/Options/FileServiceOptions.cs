namespace PartnerService.Infrastructure.Options;

public sealed class FileServiceOptions
{
    public const string SectionName = "FileService";

    public string BaseUrl { get; init; } = string.Empty;
    public string InternalApiKey { get; init; } = string.Empty;
}
