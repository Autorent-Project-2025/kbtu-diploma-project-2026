namespace PaymentService.Application.DTOs;

public sealed class PartnerWalletResponseDto
{
    public Guid PartnerUserId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal PendingAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
