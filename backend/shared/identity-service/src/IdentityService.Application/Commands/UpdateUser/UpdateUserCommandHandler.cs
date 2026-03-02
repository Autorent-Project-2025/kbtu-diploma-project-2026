using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;

namespace IdentityService.Application.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserResult> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        var normalizedUsername = command.Username.Trim();
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var existingUserByUsername = await _userRepository.GetByUsernameAsync(
            normalizedUsername,
            cancellationToken: cancellationToken);

        if (existingUserByUsername is not null && existingUserByUsername.Id != user.Id)
        {
            throw new ConflictException("Username is already in use.");
        }

        var existingUserByEmail = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken: cancellationToken);

        if (existingUserByEmail is not null && existingUserByEmail.Id != user.Id)
        {
            throw new ConflictException("Email is already in use.");
        }

        user.SetUsername(normalizedUsername);
        user.SetEmail(normalizedEmail);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        var resultUser = new UserDetailsDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            roleNames,
            permissionNames);

        return new UpdateUserResult(resultUser);
    }

    private static void Validate(UpdateUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
        {
            throw new ValidationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }
    }
}
