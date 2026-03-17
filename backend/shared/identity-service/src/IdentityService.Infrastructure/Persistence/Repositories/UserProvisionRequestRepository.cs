using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserProvisionRequestRepository : IUserProvisionRequestRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserProvisionRequestRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserProvisionRequest?> GetByRequestKeyAsync(string requestKey, CancellationToken cancellationToken = default)
    {
        var normalizedRequestKey = requestKey.Trim();
        return _dbContext.Set<UserProvisionRequest>()
            .SingleOrDefaultAsync(item => item.RequestKey == normalizedRequestKey, cancellationToken);
    }

    public async Task AddAsync(UserProvisionRequest provisionRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<UserProvisionRequest>().AddAsync(provisionRequest, cancellationToken);
    }
}
