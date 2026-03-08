using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class PartnerPayout
{
    [Column("id")]
    public long Id { get; set; }

    [Column("request_key")]
    public string RequestKey { get; set; } = string.Empty;

    [Column("partner_user_id")]
    public Guid PartnerUserId { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("status")]
    public PartnerPayoutStatus Status { get; set; } = PartnerPayoutStatus.Requested;

    [Column("requested_at")]
    public DateTimeOffset RequestedAt { get; set; }

    [Column("processed_at")]
    public DateTimeOffset? ProcessedAt { get; set; }

    [Column("failure_reason")]
    public string? FailureReason { get; set; }

    public List<PartnerLedgerEntry> LedgerEntries { get; set; } = [];
}
