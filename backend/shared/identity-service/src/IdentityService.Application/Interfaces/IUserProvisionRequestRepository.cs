using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserProvisionRequestRepository
{
    Task<UserProvisionRequest?> GetByRequestKeyAsync(string requestKey, CancellationToken cancellationToken = default);
    Task AddAsync(UserProvisionRequest provisionRequest, CancellationToken cancellationToken = default);
}
