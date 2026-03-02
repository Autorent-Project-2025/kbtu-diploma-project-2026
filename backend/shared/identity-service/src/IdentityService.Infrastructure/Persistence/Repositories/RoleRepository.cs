using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _dbContext;

    public RoleRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _dbContext.Roles.AddAsync(role, cancellationToken);
    }

    public Task<Role?> GetByIdAsync(
        Guid roleId,
        bool includePermissions = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildRoleQuery(includePermissions);
        return query.SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(
        string roleName,
        bool includePermissions = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = roleName.Trim();
        var query = BuildRoleQuery(includePermissions);
        return query.SingleOrDefaultAsync(role => role.Name == normalizedRoleName, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = roleName.Trim();
        return _dbContext.Roles.AnyAsync(role => role.Name == normalizedRoleName, cancellationToken);
    }

    private IQueryable<Role> BuildRoleQuery(bool includePermissions)
    {
        IQueryable<Role> query = _dbContext.Roles;
        if (includePermissions)
        {
            query = query.Include(role => role.Permissions);
        }

        return query;
    }
}
