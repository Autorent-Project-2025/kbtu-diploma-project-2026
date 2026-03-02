using IdentityService.Api.Contracts.Roles;
using IdentityService.Application.Commands.AssignPermissionToRole;
using IdentityService.Application.Commands.CreateRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("roles")]
public sealed class RolesController : ControllerBase
{
    private readonly CreateRoleCommandHandler _createRoleCommandHandler;
    private readonly AssignPermissionToRoleCommandHandler _assignPermissionToRoleCommandHandler;

    public RolesController(
        CreateRoleCommandHandler createRoleCommandHandler,
        AssignPermissionToRoleCommandHandler assignPermissionToRoleCommandHandler)
    {
        _createRoleCommandHandler = createRoleCommandHandler;
        _assignPermissionToRoleCommandHandler = assignPermissionToRoleCommandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createRoleCommandHandler.Handle(
            new CreateRoleCommand(request.Name),
            cancellationToken);

        return Created($"/roles/{result.RoleId}", result);
    }

    [HttpPost("{id:guid}/permissions")]
    public async Task<IActionResult> AssignPermission(
        [FromRoute] Guid id,
        [FromBody] AddPermissionToRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _assignPermissionToRoleCommandHandler.Handle(
            new AssignPermissionToRoleCommand(id, request.PermissionId),
            cancellationToken);

        return NoContent();
    }
}
