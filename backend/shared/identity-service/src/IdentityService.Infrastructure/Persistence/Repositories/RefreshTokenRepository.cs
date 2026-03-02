using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public RefreshTokenRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .ThenInclude(user => user.Roles)
            .ThenInclude(role => role.Permissions)
            .SingleOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }
}
