using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RemoveRoleFromUserCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveRoleFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        if (!user.Roles.Any(role => role.Id == command.RoleId))
        {
            throw new NotFoundException(
                $"Role '{command.RoleId}' is not assigned to user '{command.UserId}'.");
        }

        user.RemoveRole(command.RoleId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
