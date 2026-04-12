namespace TicketService.Domain.Entities;

public sealed record BookingSnapshotData
{
    public int BookingId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string CarBrand { get; init; } = string.Empty;
    public string CarModel { get; init; } = string.Empty;
    public string? PartnerName { get; init; }
    public string? CoverImageUrl { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public decimal? TotalPrice { get; init; }
    public string ReporterFullName { get; init; } = string.Empty;
    public string? ReporterPhone { get; init; }
    public string CounterpartyName { get; init; } = string.Empty;
    public Guid CounterpartyUserId { get; init; }
}
