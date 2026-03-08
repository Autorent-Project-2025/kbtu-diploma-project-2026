namespace PartnerService.Application.Models;

public sealed record PartnerLedgerEntryResult(
    long Id,
    Guid PartnerUserId,
    int? BookingId,
    long? CustomerPaymentId,
    long? PartnerPayoutId,
    string EntryType,
    string Bucket,
    decimal AmountDelta,
    string Currency,
    string? Description,
    DateTimeOffset CreatedAt);
