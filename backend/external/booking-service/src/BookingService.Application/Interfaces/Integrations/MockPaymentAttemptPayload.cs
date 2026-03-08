namespace BookingService.Application.Interfaces.Integrations
{
    public sealed class MockPaymentAttemptPayload
    {
        public long Id { get; set; }
        public int BookingId { get; set; }
        public Guid UserId { get; set; }
        public string SessionKey { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "KZT";
        public string Status { get; set; } = "started";
        public string? CardHolder { get; set; }
        public string? CardLast4 { get; set; }
        public string? FailureReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
