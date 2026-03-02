using IdentityService.Api.Contracts.Auth;
using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Commands.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerUserCommandHandler;
    private readonly LoginUserCommandHandler _loginUserCommandHandler;
    private readonly RefreshTokenCommandHandler _refreshTokenCommandHandler;

    public AuthController(
        RegisterUserCommandHandler registerUserCommandHandler,
        LoginUserCommandHandler loginUserCommandHandler,
        RefreshTokenCommandHandler refreshTokenCommandHandler)
    {
        _registerUserCommandHandler = registerUserCommandHandler;
        _loginUserCommandHandler = loginUserCommandHandler;
        _refreshTokenCommandHandler = refreshTokenCommandHandler;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerUserCommandHandler.Handle(
            new RegisterUserCommand(request.Username, request.Email, request.Password),
            cancellationToken);

        return Created($"/users/{result.UserId}", result);
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
}
