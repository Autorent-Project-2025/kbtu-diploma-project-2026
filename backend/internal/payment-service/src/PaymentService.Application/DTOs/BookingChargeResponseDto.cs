namespace PaymentService.Application.DTOs;

public sealed class BookingChargeResponseDto
{
    public long Id { get; set; }
    public int BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid PartnerUserId { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PartnerShareAmount { get; set; }
    public string Currency { get; set; } = "KZT";
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }
}
