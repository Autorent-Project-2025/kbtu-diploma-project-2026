using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;

namespace IdentityService.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRolePermissionGraphProvider _roleGraphProvider;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IRolePermissionGraphProvider roleGraphProvider)
    {
        _userRepository = userRepository;
        _roleGraphProvider = roleGraphProvider;
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

        var roleGraph = await _roleGraphProvider.GetGraphAsync(cancellationToken);
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
            user.SubjectType,
            user.ActorType,
            roleNames,
            permissionNames);

        return new GetUserByIdResult(result);
    }
}
