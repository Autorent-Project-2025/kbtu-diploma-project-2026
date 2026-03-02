using IdentityService.Api.Contracts.Internal;
using IdentityService.Application.Commands.ProvisionUser;
using IdentityService.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("internal/users")]
public sealed class InternalUsersController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly ProvisionUserCommandHandler _provisionUserCommandHandler;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalUsersController(
        ProvisionUserCommandHandler provisionUserCommandHandler,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _provisionUserCommandHandler = provisionUserCommandHandler;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("provision")]
    public async Task<IActionResult> Provision(
        [FromBody] ProvisionUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var result = await _provisionUserCommandHandler.Handle(
            new ProvisionUserCommand(request.FullName, request.Email, request.BirthDate),
            cancellationToken);

        return Ok(result);
    }

    private bool IsAuthorizedInternalRequest()
    {
        if (string.IsNullOrWhiteSpace(_internalAuthOptions.ApiKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue(InternalApiKeyHeader, out var receivedApiKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(_internalAuthOptions.ApiKey);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedApiKey.ToString());

        return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }
}
