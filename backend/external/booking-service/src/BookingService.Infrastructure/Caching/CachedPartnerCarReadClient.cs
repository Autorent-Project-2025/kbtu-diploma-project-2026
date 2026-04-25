using BookingService.Application.Interfaces.Integrations;
using Microsoft.Extensions.Caching.Memory;

namespace BookingService.Infrastructure.Caching
{
    /// <summary>
    /// Caches partner-car read responses in process memory. Reads on the
    /// booking-creation hot path go through here so a burst of bookings for
    /// the same car (~13 cars in the load test) doesn't fan out to car-service
    /// on every iteration.
    ///
    /// Cache keys bucket the time window to the hour to maximize hit rate
    /// while keeping pricing context responsive to changes within the day.
    /// TTL is intentionally short (30s) — data freshness matters more than
    /// hit rate, and the upstream call is fast enough that 30s of churn is
    /// acceptable.
    /// </summary>
    public sealed class CachedPartnerCarReadClient : IPartnerCarReadClient
    {
        private static readonly TimeSpan PricingContextTtl = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan SnapshotTtl = TimeSpan.FromSeconds(60);

        private readonly IPartnerCarReadClient _inner;
        private readonly IMemoryCache _cache;

        public CachedPartnerCarReadClient(IPartnerCarReadClient inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<PartnerCarPricingContext?> GetPricingContextAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            var key = $"pricing-context:{partnerCarId}:{startTime:yyyyMMddHH}:{endTime:yyyyMMddHH}";

            if (_cache.TryGetValue(key, out PartnerCarPricingContext? cached))
            {
                return cached;
            }

            var result = await _inner.GetPricingContextAsync(partnerCarId, startTime, endTime, cancellationToken);

            // Cache only successful lookups — null/missing context is usually
            // a transient state that the next caller should re-resolve.
            if (result is not null)
            {
                _cache.Set(key, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = PricingContextTtl,
                });
            }

            return result;
        }

        public async Task<PartnerCarSnapshotPayload?> GetSnapshotAsync(
            int partnerCarId,
            CancellationToken cancellationToken = default)
        {
            var key = $"partner-car-snapshot:{partnerCarId}";

            if (_cache.TryGetValue(key, out PartnerCarSnapshotPayload? cached))
            {
                return cached;
            }

            var result = await _inner.GetSnapshotAsync(partnerCarId, cancellationToken);

            if (result is not null)
            {
                _cache.Set(key, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = SnapshotTtl,
                });
            }

            return result;
        }
    }
}
