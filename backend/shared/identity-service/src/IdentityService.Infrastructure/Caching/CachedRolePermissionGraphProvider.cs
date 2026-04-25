using IdentityService.Application.Interfaces;
using IdentityService.Application.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Infrastructure.Caching;

/// <summary>
/// Caches the resolved role-permission graph in process memory. The graph
/// is fully immutable (RoleGraphNode is a record of read-only collections),
/// so it's safe to share across requests and threads. A static semaphore
/// dedupes concurrent rebuilds when the cache is cold — otherwise a burst
/// of logins would fan out N parallel "load all roles + permissions"
/// queries before the first one finishes.
/// </summary>
public sealed class CachedRolePermissionGraphProvider : IRolePermissionGraphProvider
{
    private const string CacheKey = "identity:role-permission-graph";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    // Static so a single in-flight load is shared across all scoped
    // provider instances inside one process.
    private static readonly SemaphoreSlim Gate = new(initialCount: 1, maxCount: 1);

    private readonly IMemoryCache _cache;
    private readonly IRoleRepository _roleRepository;

    public CachedRolePermissionGraphProvider(
        IMemoryCache cache,
        IRoleRepository roleRepository)
    {
        _cache = cache;
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyDictionary<Guid, RoleGraphNode>> GetGraphAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<Guid, RoleGraphNode>? cached)
            && cached is not null)
        {
            return cached;
        }

        await Gate.WaitAsync(cancellationToken);
        try
        {
            // Re-check under the gate: a concurrent loader may have already
            // populated the cache while we were waiting.
            if (_cache.TryGetValue(CacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var roles = await _roleRepository.ListAsync(
                includePermissions: true,
                includeParentRoles: true,
                cancellationToken: cancellationToken);

            var graph = RolePermissionResolver.BuildGraph(roles);

            _cache.Set(
                CacheKey,
                graph,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheTtl,
                });

            return graph;
        }
        finally
        {
            Gate.Release();
        }
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }
}
