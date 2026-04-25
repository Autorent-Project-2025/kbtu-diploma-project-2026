using BookingService.Application.Interfaces.Integrations;
using Microsoft.Extensions.Caching.Memory;

namespace BookingService.Infrastructure.Caching
{
    /// <summary>
    /// Caches the booking-access lookup that runs on every booking creation.
    /// The block flag changes only when an admin blocks/unblocks a user, so
    /// 30s staleness is acceptable. Mutation paths (SetBookingActionsBlockedAsync)
    /// invalidate the cached entry inline so the new state is reflected on
    /// the next read instead of waiting for TTL.
    /// </summary>
    public sealed class CachedClientBookingAccessClient : IClientBookingAccessClient
    {
        private static readonly TimeSpan AccessTtl = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ProfileTtl = TimeSpan.FromSeconds(30);

        private readonly IClientBookingAccessClient _inner;
        private readonly IMemoryCache _cache;

        public CachedClientBookingAccessClient(IClientBookingAccessClient inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<ClientBookingAccessPayload?> GetBookingAccessAsync(
            Guid relatedUserId,
            CancellationToken cancellationToken = default)
        {
            var key = AccessKey(relatedUserId);

            if (_cache.TryGetValue(key, out ClientBookingAccessPayload? cached))
            {
                return cached;
            }

            var result = await _inner.GetBookingAccessAsync(relatedUserId, cancellationToken);

            if (result is not null)
            {
                _cache.Set(key, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AccessTtl,
                });
            }

            return result;
        }

        public async Task<ClientProfilePayload?> GetClientProfileAsync(
            Guid relatedUserId,
            CancellationToken cancellationToken = default)
        {
            var key = ProfileKey(relatedUserId);

            if (_cache.TryGetValue(key, out ClientProfilePayload? cached))
            {
                return cached;
            }

            var result = await _inner.GetClientProfileAsync(relatedUserId, cancellationToken);

            if (result is not null)
            {
                _cache.Set(key, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ProfileTtl,
                });
            }

            return result;
        }

        public async Task<ClientProfilePayload?> SetBookingActionsBlockedAsync(
            Guid relatedUserId,
            bool isBlocked,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var result = await _inner.SetBookingActionsBlockedAsync(relatedUserId, isBlocked, reason, cancellationToken);

            // Block-state mutation invalidates both reads for this user so the
            // next request observes the change without waiting for TTL.
            _cache.Remove(AccessKey(relatedUserId));
            _cache.Remove(ProfileKey(relatedUserId));

            return result;
        }

        private static string AccessKey(Guid userId) => $"client-booking-access:{userId}";
        private static string ProfileKey(Guid userId) => $"client-profile:{userId}";
    }
}
