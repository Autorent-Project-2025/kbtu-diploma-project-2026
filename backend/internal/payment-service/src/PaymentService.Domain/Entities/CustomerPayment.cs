using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class CustomerPayment
{
    [Column("id")]
    public long Id { get; set; }

    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("partner_user_id")]
    public Guid PartnerUserId { get; set; }

    [Column("partner_car_id")]
    public int PartnerCarId { get; set; }

    [Column("price_hour")]
    public decimal? PriceHour { get; set; }

    [Column("gross_amount")]
    public decimal GrossAmount { get; set; }

    [Column("platform_commission_rate")]
    public decimal PlatformCommissionRate { get; set; }

    [Column("platform_commission_amount")]
    public decimal PlatformCommissionAmount { get; set; }

    [Column("partner_amount")]
    public decimal PartnerAmount { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("status")]
    public CustomerPaymentStatus Status { get; set; } = CustomerPaymentStatus.Pending;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [Column("confirmed_at")]
    public DateTimeOffset ConfirmedAt { get; set; }

    [Column("available_at")]
    public DateTimeOffset? AvailableAt { get; set; }

    [Column("canceled_at")]
    public DateTimeOffset? CanceledAt { get; set; }

    public List<PartnerLedgerEntry> LedgerEntries { get; set; } = [];
}
