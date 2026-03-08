using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class PartnerLedgerEntry
{
    [Column("id")]
    public long Id { get; set; }

    [Column("partner_wallet_id")]
    public long PartnerWalletId { get; set; }

    [Column("booking_id")]
    public int? BookingId { get; set; }

    [Column("customer_payment_id")]
    public long? CustomerPaymentId { get; set; }

    [Column("partner_payout_id")]
    public long? PartnerPayoutId { get; set; }

    [Column("entry_type")]
    public LedgerEntryType EntryType { get; set; }

    [Column("bucket")]
    public LedgerBucket Bucket { get; set; }

    [Column("amount_delta")]
    public decimal AmountDelta { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public PartnerWallet PartnerWallet { get; set; } = null!;
    public CustomerPayment? CustomerPayment { get; set; }
    public PartnerPayout? PartnerPayout { get; set; }
}
