using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IActivationTokenRepository
{
    Task AddAsync(ActivationToken activationToken, CancellationToken cancellationToken = default);

    Task<ActivationToken?> GetByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
