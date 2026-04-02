using IdentityService.Api.Contracts.Internal;
using IdentityService.Application.Commands.ProvisionUser;
using IdentityService.Application.Queries.GetUserById;
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
    private readonly GetUserByIdQueryHandler _getUserByIdQueryHandler;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalUsersController(
        ProvisionUserCommandHandler provisionUserCommandHandler,
        GetUserByIdQueryHandler getUserByIdQueryHandler,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _provisionUserCommandHandler = provisionUserCommandHandler;
        _getUserByIdQueryHandler = getUserByIdQueryHandler;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var result = await _getUserByIdQueryHandler.Handle(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result.User);
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
            new ProvisionUserCommand(
                request.FullName,
                request.Email,
                request.BirthDate,
                request.RequestKey,
                request.SubjectType,
                request.ActorType),
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
