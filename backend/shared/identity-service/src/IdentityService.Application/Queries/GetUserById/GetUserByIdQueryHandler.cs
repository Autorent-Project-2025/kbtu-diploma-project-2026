using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;

namespace IdentityService.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<GetUserByIdResult> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            query.UserId,
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User '{query.UserId}' was not found.");
        }

        var roles = await _roleRepository.ListAsync(
            includePermissions: true,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        var roleGraph = RolePermissionResolver.BuildGraph(roles);
        var roleNames = user.Roles
            .Select(role => role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = RolePermissionResolver.ResolveEffectivePermissions(
            user.Roles.Select(role => role.Id),
            roleGraph);

        var result = new UserDetailsDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            roleNames,
            permissionNames);

        return new GetUserByIdResult(result);
    }
}
