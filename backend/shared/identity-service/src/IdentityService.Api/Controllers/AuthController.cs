using IdentityService.Api.Contracts.Auth;
using IdentityService.Application.Commands.ActivateUser;
using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Queries.GetActivationTokenStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUserCommandHandler _loginUserCommandHandler;
    private readonly RefreshTokenCommandHandler _refreshTokenCommandHandler;
    private readonly ActivateUserCommandHandler _activateUserCommandHandler;
    private readonly GetActivationTokenStatusQueryHandler _getActivationTokenStatusQueryHandler;

    public AuthController(
        LoginUserCommandHandler loginUserCommandHandler,
        RefreshTokenCommandHandler refreshTokenCommandHandler,
        ActivateUserCommandHandler activateUserCommandHandler,
        GetActivationTokenStatusQueryHandler getActivationTokenStatusQueryHandler)
    {
        _loginUserCommandHandler = loginUserCommandHandler;
        _refreshTokenCommandHandler = refreshTokenCommandHandler;
        _activateUserCommandHandler = activateUserCommandHandler;
        _getActivationTokenStatusQueryHandler = getActivationTokenStatusQueryHandler;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _loginUserCommandHandler.Handle(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _refreshTokenCommandHandler.Handle(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("activation-status")]
    public async Task<IActionResult> GetActivationStatus(
        [FromQuery] string activationToken,
        CancellationToken cancellationToken)
    {
        var result = await _getActivationTokenStatusQueryHandler.Handle(
            new GetActivationTokenStatusQuery(activationToken),
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("activate")]
    public async Task<IActionResult> Activate(
        [FromBody] ActivateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _activateUserCommandHandler.Handle(
            new ActivateUserCommand(request.ActivationToken, request.Password),
            cancellationToken);

        return Ok(result);
    }
}
