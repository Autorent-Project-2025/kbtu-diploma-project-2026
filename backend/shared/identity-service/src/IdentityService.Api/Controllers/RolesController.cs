using IdentityService.Api.Contracts.Roles;
using IdentityService.Application.Commands.AssignPermissionToRole;
using IdentityService.Application.Commands.CreateRole;
using IdentityService.Application.Queries.GetRoles;
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
    private readonly GetRolesQueryHandler _getRolesQueryHandler;

    public RolesController(
        CreateRoleCommandHandler createRoleCommandHandler,
        AssignPermissionToRoleCommandHandler assignPermissionToRoleCommandHandler,
        GetRolesQueryHandler getRolesQueryHandler)
    {
        _createRoleCommandHandler = createRoleCommandHandler;
        _assignPermissionToRoleCommandHandler = assignPermissionToRoleCommandHandler;
        _getRolesQueryHandler = getRolesQueryHandler;
    }

    [HttpGet]
    [Authorize(Policy = "roles:view")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _getRolesQueryHandler.Handle(new GetRolesQuery(), cancellationToken);
        return Ok(result.Roles);
    }

    [HttpPost]
    [Authorize(Policy = "roles:create")]
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
    [Authorize(Policy = "roles:assign-permission")]
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
