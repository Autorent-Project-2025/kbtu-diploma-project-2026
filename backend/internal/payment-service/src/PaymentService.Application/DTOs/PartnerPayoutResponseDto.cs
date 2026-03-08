namespace PaymentService.Application.DTOs;

public sealed class PartnerPayoutResponseDto
{
    public long Id { get; set; }
    public string RequestKey { get; set; } = string.Empty;
    public Guid PartnerUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KZT";
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
}
