using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
