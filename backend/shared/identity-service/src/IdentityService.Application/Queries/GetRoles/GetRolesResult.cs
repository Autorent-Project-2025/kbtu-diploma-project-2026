using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetRoles;

public sealed record GetRolesResult(IReadOnlyCollection<RoleDetailsDto> Roles);
