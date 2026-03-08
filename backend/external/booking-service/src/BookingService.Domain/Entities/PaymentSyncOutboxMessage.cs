using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities
{
    public class PaymentSyncOutboxMessage
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("booking_id")]
        public int BookingId { get; set; }

        [Column("event_key")]
        public string EventKey { get; set; } = string.Empty;

        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        [Column("payload")]
        public string Payload { get; set; } = "{}";

        [Column("attempt_count")]
        public int AttemptCount { get; set; }

        [Column("last_error")]
        public string? LastError { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("next_attempt_at")]
        public DateTimeOffset NextAttemptAt { get; set; }

        [Column("processed_at")]
        public DateTimeOffset? ProcessedAt { get; set; }

        [Column("locked_until")]
        public DateTimeOffset? LockedUntil { get; set; }
    }
}
