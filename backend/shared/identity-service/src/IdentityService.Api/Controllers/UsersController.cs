using IdentityService.Api.Contracts.Users;
using IdentityService.Application.Commands.AssignRoleToUser;
using IdentityService.Application.Commands.CreateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly AssignRoleToUserCommandHandler _assignRoleToUserCommandHandler;
    private readonly CreateUserCommandHandler _createUserCommandHandler;

    public UsersController(
        AssignRoleToUserCommandHandler assignRoleToUserCommandHandler,
        CreateUserCommandHandler createUserCommandHandler)
    {
        _assignRoleToUserCommandHandler = assignRoleToUserCommandHandler;
        _createUserCommandHandler = createUserCommandHandler;
    }

    [HttpPost]
    [Authorize(Policy = "users:create")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createUserCommandHandler.Handle(
            new CreateUserCommand(request.Username, request.Email, request.Password, request.Roles),
            cancellationToken);

        return Created($"/users/{result.UserId}", result);
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = "users:assign-role")]
    public async Task<IActionResult> AssignRoleToUser(
        [FromRoute] Guid id,
        [FromBody] AssignRoleToUserRequest request,
        CancellationToken cancellationToken)
    {
        await _assignRoleToUserCommandHandler.Handle(
            new AssignRoleToUserCommand(id, request.RoleId),
            cancellationToken);

        return NoContent();
    }
}
