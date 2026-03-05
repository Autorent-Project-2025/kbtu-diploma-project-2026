using IdentityService.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Queries.GetActivationTokenStatus;

public sealed class GetActivationTokenStatusQueryHandler
{
    private readonly IActivationTokenRepository _activationTokenRepository;

    public GetActivationTokenStatusQueryHandler(IActivationTokenRepository activationTokenRepository)
    {
        _activationTokenRepository = activationTokenRepository;
    }

    public async Task<GetActivationTokenStatusResult> Handle(
        GetActivationTokenStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.ActivationToken))
        {
            return new GetActivationTokenStatusResult(false, null, "invalid");
        }

        var tokenHash = ComputeTokenHash(query.ActivationToken);
        var nowUtc = DateTime.UtcNow;

        var activationToken = await _activationTokenRepository.GetByTokenHashWithUserAsync(tokenHash, cancellationToken);
        if (activationToken is null)
        {
            return new GetActivationTokenStatusResult(false, null, "invalid");
        }

        if (activationToken.UsedAtUtc is not null)
        {
            return new GetActivationTokenStatusResult(false, activationToken.ExpiresAtUtc, "used");
        }

        if (activationToken.ExpiresAtUtc <= nowUtc)
        {
            return new GetActivationTokenStatusResult(false, activationToken.ExpiresAtUtc, "expired");
        }

        return new GetActivationTokenStatusResult(true, activationToken.ExpiresAtUtc, null);
    }

    private static string ComputeTokenHash(string activationToken)
    {
        var normalizedToken = activationToken.Trim();
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedToken));
        return Convert.ToHexString(hashBytes);
    }
}
