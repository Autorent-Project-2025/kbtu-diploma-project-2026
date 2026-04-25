using IdentityService.Application.Utils;

namespace IdentityService.Application.Interfaces;

/// <summary>
/// Provides access to the resolved role-permission graph used during
/// authentication and authorization. Implementations are expected to cache
/// the graph because building it requires loading every role with its
/// permissions and parent links — work that's identical across all logins
/// and only changes when an admin mutates the role/permission structure.
/// </summary>
public interface IRolePermissionGraphProvider
{
    Task<IReadOnlyDictionary<Guid, RoleGraphNode>> GetGraphAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the cached graph. Mutation handlers that change roles,
    /// permissions, or role-parent links must call this after a successful
    /// SaveChangesAsync so the next read rebuilds from the database.
    /// </summary>
    void Invalidate();
}
