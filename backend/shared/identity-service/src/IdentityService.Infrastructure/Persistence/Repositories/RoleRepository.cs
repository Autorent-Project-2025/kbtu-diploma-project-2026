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

    public async Task<IReadOnlyCollection<Role>> ListAsync(
        bool includePermissions = false,
        bool includeParentRoles = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildRoleQuery(includePermissions, includeParentRoles);
        return await query
            .OrderBy(role => role.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Role?> GetByIdAsync(
        Guid roleId,
        bool includePermissions = false,
        bool includeParentRoles = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildRoleQuery(includePermissions, includeParentRoles);
        return query.SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(
        string roleName,
        bool includePermissions = false,
        bool includeParentRoles = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = roleName.Trim();
        var query = BuildRoleQuery(includePermissions, includeParentRoles);
        return query.SingleOrDefaultAsync(role => role.Name == normalizedRoleName, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = roleName.Trim();
        return _dbContext.Roles.AnyAsync(role => role.Name == normalizedRoleName, cancellationToken);
    }

    private IQueryable<Role> BuildRoleQuery(bool includePermissions, bool includeParentRoles)
    {
        IQueryable<Role> query = _dbContext.Roles;
        if (includePermissions)
        {
            query = query.Include(role => role.Permissions);
        }

        if (includeParentRoles)
        {
            query = query.Include(role => role.ParentRoles);
        }

        return query;
    }
}
