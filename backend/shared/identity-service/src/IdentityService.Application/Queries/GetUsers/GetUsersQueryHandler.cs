using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUsersResult> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAsync(
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        var result = users.Select(MapUser).ToArray();
        return new GetUsersResult(result);
    }

    private static UserDetailsDto MapUser(User user)
    {
        var roleNames = user.Roles
            .Select(role => role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = user.Roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserDetailsDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            roleNames,
            permissionNames);
    }
}
