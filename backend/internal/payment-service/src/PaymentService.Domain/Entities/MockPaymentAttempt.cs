using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public sealed class MockPaymentAttempt
{
    [Column("id")]
    public long Id { get; set; }

    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("session_key")]
    public string SessionKey { get; set; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("currency")]
    public string Currency { get; set; } = "KZT";

    [Column("status")]
    public MockPaymentAttemptStatus Status { get; set; } = MockPaymentAttemptStatus.Started;

    [Column("card_holder")]
    public string? CardHolder { get; set; }

    [Column("card_last4")]
    public string? CardLast4 { get; set; }

    [Column("failure_reason")]
    public string? FailureReason { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
}
