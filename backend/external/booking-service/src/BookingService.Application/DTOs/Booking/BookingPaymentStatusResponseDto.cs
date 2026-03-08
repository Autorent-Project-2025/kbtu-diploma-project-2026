namespace BookingService.Application.DTOs.Booking
{
    public sealed class BookingPaymentStatusResponseDto
    {
        public int BookingId { get; set; }
        public string BookingStatus { get; set; } = "pending";
        public string PaymentStatus { get; set; } = "not_started";
        public long? PaymentAttemptId { get; set; }
        public string? SessionKey { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; } = "KZT";
        public string? CardHolder { get; set; }
        public string? CardLast4 { get; set; }
        public string? FailureReason { get; set; }
        public DateTimeOffset? PaymentCreatedAt { get; set; }
        public DateTimeOffset? PaymentUpdatedAt { get; set; }
        public DateTimeOffset? PaymentCompletedAt { get; set; }
        public DateTimeOffset? PaymentExpiresAt { get; set; }
        public bool RequiresInput { get; set; }
        public bool CanRetry { get; set; }
    }
}
