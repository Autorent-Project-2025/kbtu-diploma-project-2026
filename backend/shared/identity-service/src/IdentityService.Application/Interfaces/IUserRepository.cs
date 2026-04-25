using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> ListAsync(
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tight, login-path-only user load: pulls just User + Roles (without
    /// permissions) and skips EF change tracking. The role-permission graph
    /// is supplied by IRolePermissionGraphProvider, so we don't need to drag
    /// permission rows over the wire on every authentication.
    /// </summary>
    Task<User?> GetForLoginAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(
        string username,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        Guid userId,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    void Delete(User user);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
