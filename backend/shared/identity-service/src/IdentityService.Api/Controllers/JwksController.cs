using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Api.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class JwksController : ControllerBase
{
    private readonly IJwtProvider _jwtProvider;

    public JwksController(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    [HttpGet("/.well-known/jwks.json")]
    public IActionResult GetJwks()
    {
        var publicKeyPem = _jwtProvider.GetPublicKeyPem();
        var publicKeyParameters = RsaKeyMaterial.ReadPublicKeyParameters(publicKeyPem);

        if (publicKeyParameters.Modulus is null || publicKeyParameters.Exponent is null)
        {
            throw new InvalidOperationException("Public RSA key is invalid.");
        }

        var jwk = new
        {
            kty = "RSA",
            use = "sig",
            kid = _jwtProvider.GetKeyId(),
            alg = "RS256",
            n = Base64UrlEncoder.Encode(publicKeyParameters.Modulus),
            e = Base64UrlEncoder.Encode(publicKeyParameters.Exponent)
        };

        return Ok(new { keys = new[] { jwk } });
    }
}
