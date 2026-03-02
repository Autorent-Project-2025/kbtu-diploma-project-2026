using IdentityService.Application.Models;

namespace IdentityService.Application.Interfaces;

public interface IJwtProvider
{
    AccessTokenResult GenerateAccessToken(
        Guid userId,
        string username,
        IReadOnlyCollection<string> permissions);

    RefreshTokenResult GenerateRefreshToken();
    string ComputeRefreshTokenHash(string refreshToken);

    string GetPublicKeyPem();
    string GetKeyId();
}
