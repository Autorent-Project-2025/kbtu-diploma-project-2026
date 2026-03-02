using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}
