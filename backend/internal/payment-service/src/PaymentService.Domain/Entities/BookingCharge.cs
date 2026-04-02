using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public sealed class BookingCharge
{
    [Column("id")]
    public long Id { get; set; }

    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("partner_user_id")]
    public Guid PartnerUserId { get; set; }

    [Column("charge_type")]
    public BookingChargeType ChargeType { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("partner_share_amount")]
    public decimal PartnerShareAmount { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("status")]
    public BookingChargeStatus Status { get; set; } = BookingChargeStatus.Pending;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [Column("paid_at")]
    public DateTimeOffset? PaidAt { get; set; }

    [Column("canceled_at")]
    public DateTimeOffset? CanceledAt { get; set; }
}
