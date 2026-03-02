using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly IdentityDbContext _dbContext;

    public PermissionRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _dbContext.Permissions.AddAsync(permission, cancellationToken);
    }

    public Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Permissions.SingleOrDefaultAsync(
            permission => permission.Id == permissionId,
            cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string permissionName, CancellationToken cancellationToken = default)
    {
        var normalizedPermissionName = permissionName.Trim();
        return _dbContext.Permissions.AnyAsync(
            permission => permission.Name == normalizedPermissionName,
            cancellationToken);
    }
}
