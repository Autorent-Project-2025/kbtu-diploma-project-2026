namespace BookingService.Application.Interfaces.Integrations;

public sealed class FileUploadPayload
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public byte[] Content { get; init; } = [];
}
