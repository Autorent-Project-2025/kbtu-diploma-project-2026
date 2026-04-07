namespace CarService.Api.Contracts.Internal;

public sealed class CreateCarCommentRequest
{
    public int BookingId { get; init; }
    public int PartnerCarId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Content { get; init; } = string.Empty;
}
