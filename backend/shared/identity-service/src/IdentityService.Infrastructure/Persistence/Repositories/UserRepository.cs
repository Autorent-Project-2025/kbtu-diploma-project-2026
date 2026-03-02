using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildUserQuery(includeRolesAndPermissions);
        return await query
            .OrderBy(user => user.Username)
            .ToArrayAsync(cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = BuildUserQuery(includeRolesAndPermissions);

        return query.SingleOrDefaultAsync(
            user => user.Email == normalizedEmail,
            cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(
        string username,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
        var query = BuildUserQuery(includeRolesAndPermissions);
        return query.SingleOrDefaultAsync(user => user.Username == normalizedUsername, cancellationToken);
    }

    public Task<User?> GetByIdAsync(
        Guid userId,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildUserQuery(includeRolesAndPermissions);
        return query.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public void Delete(User user)
    {
        _dbContext.Users.Remove(user);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
        return _dbContext.Users.AnyAsync(user => user.Username == normalizedUsername, cancellationToken);
    }

    private IQueryable<User> BuildUserQuery(bool includeRolesAndPermissions)
    {
        IQueryable<User> query = _dbContext.Users;
        if (includeRolesAndPermissions)
        {
            query = query
                .Include(user => user.Roles)
                .ThenInclude(role => role.Permissions);
        }

        return query;
    }
}
