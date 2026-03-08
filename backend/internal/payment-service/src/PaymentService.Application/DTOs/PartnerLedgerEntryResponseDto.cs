namespace PaymentService.Application.DTOs;

public sealed class PartnerLedgerEntryResponseDto
{
    public long Id { get; set; }
    public Guid PartnerUserId { get; set; }
    public int? BookingId { get; set; }
    public long? CustomerPaymentId { get; set; }
    public long? PartnerPayoutId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public decimal AmountDelta { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
