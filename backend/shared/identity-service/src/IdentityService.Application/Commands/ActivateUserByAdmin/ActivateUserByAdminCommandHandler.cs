using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.ActivateUserByAdmin;

public sealed class ActivateUserByAdminCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ActivateUserByAdminCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ActivateUserByAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        user.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
