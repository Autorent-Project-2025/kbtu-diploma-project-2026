using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Domain.Entities;

public class PartnerWallet
{
    [Column("id")]
    public long Id { get; set; }

    [Column("partner_user_id")]
    public Guid PartnerUserId { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("pending_amount")]
    public decimal PendingAmount { get; set; }

    [Column("available_amount")]
    public decimal AvailableAmount { get; set; }

    [Column("reserved_amount")]
    public decimal ReservedAmount { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    public List<PartnerLedgerEntry> LedgerEntries { get; set; } = [];
}
