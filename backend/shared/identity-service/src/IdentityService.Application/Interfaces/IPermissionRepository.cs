using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IPermissionRepository
{
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Permission>> ListAsync(CancellationToken cancellationToken = default);
    Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string permissionName, CancellationToken cancellationToken = default);
}
