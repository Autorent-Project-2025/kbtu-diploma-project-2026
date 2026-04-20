namespace BookingService.Infrastructure.Options
{
    public sealed class DamageEvalServiceOptions
    {
        public const string SectionName = "DamageEvalService";

        public string BaseUrl { get; set; } = string.Empty;
        public string InternalApiKey { get; set; } = string.Empty;

        // Total per-request budget for the synchronous AI call issued from
        // SubmitCompletionReview. We intentionally use a generous default
        // because model inference can take 20-40s on CPU. On GPU the call
        // completes in under 2s, so this budget is almost never reached.
        public int TimeoutSeconds { get; set; } = 60;
    }
}
