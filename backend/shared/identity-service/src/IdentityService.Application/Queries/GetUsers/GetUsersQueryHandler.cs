using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRolePermissionGraphProvider _roleGraphProvider;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        IRolePermissionGraphProvider roleGraphProvider)
    {
        _userRepository = userRepository;
        _roleGraphProvider = roleGraphProvider;
    }

    public async Task<GetUsersResult> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAsync(
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        var roleGraph = await _roleGraphProvider.GetGraphAsync(cancellationToken);
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
            user.SubjectType,
            user.ActorType,
            roleNames,
            permissionNames);
    }
}
