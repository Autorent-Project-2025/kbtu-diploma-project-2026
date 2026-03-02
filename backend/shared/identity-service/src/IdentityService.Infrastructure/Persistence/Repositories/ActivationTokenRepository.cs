using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class ActivationTokenRepository : IActivationTokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public ActivationTokenRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ActivationToken activationToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.ActivationTokens.AddAsync(activationToken, cancellationToken);
    }

    public Task<ActivationToken?> GetByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var normalizedTokenHash = tokenHash.Trim();

        return _dbContext.ActivationTokens
            .Include(activationToken => activationToken.User)
            .SingleOrDefaultAsync(activationToken => activationToken.TokenHash == normalizedTokenHash, cancellationToken);
    }
}
