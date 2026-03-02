using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(
        Guid roleId,
        bool includePermissions = false,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByNameAsync(
        string roleName,
        bool includePermissions = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
