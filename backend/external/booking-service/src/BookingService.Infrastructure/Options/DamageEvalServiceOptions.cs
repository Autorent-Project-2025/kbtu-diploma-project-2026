namespace BookingService.Infrastructure.Options
{
    public sealed class DamageEvalServiceOptions
    {
        public const string SectionName = "DamageEvalService";

        public string BaseUrl { get; set; } = string.Empty;
        public string InternalApiKey { get; set; } = string.Empty;

        // Hard upper bound on the synchronous AI call from
        // SubmitCompletionReview. MUST stay materially below the gateway
        // timeout (60s) so the booking flow fails open BEFORE the client
        // sees an opaque gateway 504.
        //
        // 15s default: covers GPU inference (~2s per photo × 5 = 10s)
        // plus network / multipart upload overhead with headroom. On
        // CPU-only deployments this will frequently time out — that's
        // by design: we'd rather fail open than make the user wait for
        // advisory AI when it's slow.
        public int TimeoutSeconds { get; set; } = 15;
    }
}
