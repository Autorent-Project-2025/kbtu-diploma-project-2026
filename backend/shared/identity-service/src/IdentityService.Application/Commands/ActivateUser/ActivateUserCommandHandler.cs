using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler
{
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ActivateUserCommandHandler(
        IActivationTokenRepository activationTokenRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _activationTokenRepository = activationTokenRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActivateUserResult> Handle(
        ActivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var tokenHash = ComputeTokenHash(command.ActivationToken);
        var nowUtc = DateTime.UtcNow;

        var activationToken = await _activationTokenRepository.GetByTokenHashWithUserAsync(tokenHash, cancellationToken);
        if (activationToken is null || !activationToken.IsActive(nowUtc) || activationToken.User is null)
        {
            throw new UnauthorizedException("Activation token is invalid or expired.");
        }

        activationToken.User.SetPasswordHash(_passwordHasher.Hash(command.Password.Trim()));
        activationToken.MarkAsUsed(nowUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivateUserResult(activationToken.User.Id, activationToken.User.Email);
    }

    private static void Validate(ActivateUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ActivationToken))
        {
            throw new ValidationException("Activation token is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ValidationException("Password is required.");
        }
    }

    private static string ComputeTokenHash(string activationToken)
    {
        var normalizedToken = activationToken.Trim();
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedToken));
        return Convert.ToHexString(hashBytes);
    }
}
