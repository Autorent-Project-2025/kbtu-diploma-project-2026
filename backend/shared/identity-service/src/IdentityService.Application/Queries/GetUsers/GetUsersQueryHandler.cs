using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public GetUsersQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<GetUsersResult> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAsync(
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        var roles = await _roleRepository.ListAsync(
            includePermissions: true,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        var roleGraph = RolePermissionResolver.BuildGraph(roles);
        var result = users.Select(user => MapUser(user, roleGraph)).ToArray();
        return new GetUsersResult(result);
    }

    private static UserDetailsDto MapUser(
        User user,
        IReadOnlyDictionary<Guid, RoleGraphNode> roleGraph)
    {
        var roleNames = user.Roles
            .Select(role => role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = RolePermissionResolver.ResolveEffectivePermissions(
            user.Roles.Select(role => role.Id),
            roleGraph);

        return new UserDetailsDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            roleNames,
            permissionNames);
    }
}
