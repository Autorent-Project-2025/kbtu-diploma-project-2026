using IdentityService.Api.Contracts.Users;
using IdentityService.Application.Commands.AssignRoleToUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly AssignRoleToUserCommandHandler _assignRoleToUserCommandHandler;

    public UsersController(AssignRoleToUserCommandHandler assignRoleToUserCommandHandler)
    {
        _assignRoleToUserCommandHandler = assignRoleToUserCommandHandler;
    }

    [HttpPost("{id:guid}/roles")]
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
