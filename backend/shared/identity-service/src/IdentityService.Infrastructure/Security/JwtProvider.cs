using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Security;

public sealed class JwtProvider : IJwtProvider, IDisposable
{
    private readonly JwtOptions _options;
    private readonly RSA _rsa;
    private readonly SigningCredentials _signingCredentials;
    private readonly string _publicKeyPem;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.PrivateKey))
        {
            throw new InvalidOperationException("Configuration value 'Jwt:PrivateKey' is required.");
        }

        var normalizedPrivateKey = RsaKeyMaterial.NormalizePem(_options.PrivateKey);
        _publicKeyPem = string.IsNullOrWhiteSpace(_options.PublicKey)
            ? RsaKeyMaterial.DerivePublicKeyPem(normalizedPrivateKey)
            : RsaKeyMaterial.NormalizePem(_options.PublicKey);

        _rsa = RSA.Create();
        _rsa.ImportFromPem(normalizedPrivateKey);

        var signingKey = new RsaSecurityKey(_rsa)
        {
            KeyId = _options.KeyId
        };

        _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
    }

    public AccessTokenResult GenerateAccessToken(
        Guid userId,
        string username,
        IReadOnlyCollection<string> permissions)
    {
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenLifetimeMinutes <= 0 ? 15 : _options.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("username", username)
        };

        foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("permissions", permission));
        }

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return new AccessTokenResult(token, expiresAtUtc);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Base64UrlEncoder.Encode(randomBytes);

        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddDays(_options.RefreshTokenLifetimeDays <= 0 ? 30 : _options.RefreshTokenLifetimeDays);

        return new RefreshTokenResult(token, expiresAtUtc);
    }

    public string ComputeRefreshTokenHash(string refreshToken)
    {
        var normalizedToken = refreshToken?.Trim() ?? string.Empty;
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedToken));
        return Convert.ToHexString(hashBytes);
    }

    public string GetPublicKeyPem()
    {
        return _publicKeyPem;
    }

    public string GetKeyId()
    {
        return _options.KeyId;
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}
