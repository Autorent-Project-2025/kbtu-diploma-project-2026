using IdentityService.Application.Constants;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Commands.ProvisionUser;

public sealed class ProvisionUserCommandHandler
{
    private static readonly char[] NonAlphaNumericCharacters = ['-', '_', '.'];

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ProvisionUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IActivationTokenRepository activationTokenRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _activationTokenRepository = activationTokenRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProvisionUserResult> Handle(
        ProvisionUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("Email is already in use.");
        }

        var username = await BuildUniqueUsernameAsync(command, cancellationToken);
        var userRole = await _roleRepository.GetByNameAsync(RoleConstants.User, cancellationToken: cancellationToken);
        if (userRole is null)
        {
            throw new NotFoundException($"Default role '{RoleConstants.User}' was not found.");
        }

        var temporaryPassword = $"temp-{Guid.NewGuid():N}";
        var user = new User(Guid.NewGuid(), username, normalizedEmail, _passwordHasher.Hash(temporaryPassword));
        user.AssignRole(userRole);

        var nowUtc = DateTime.UtcNow;
        var activationToken = GenerateToken();
        var activationTokenHash = ComputeTokenHash(activationToken);
        var activationExpiresAtUtc = nowUtc.AddHours(24);

        var activationTokenEntity = new ActivationToken(
            Guid.NewGuid(),
            user.Id,
            activationTokenHash,
            nowUtc,
            activationExpiresAtUtc);

        await _userRepository.AddAsync(user, cancellationToken);
        await _activationTokenRepository.AddAsync(activationTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProvisionUserResult(
            user.Id,
            user.Username,
            user.Email,
            activationToken,
            activationExpiresAtUtc);
    }

    private async Task<string> BuildUniqueUsernameAsync(
        ProvisionUserCommand command,
        CancellationToken cancellationToken)
    {
        var emailLocalPart = command.Email.Split('@')[0];
        var fullNameNormalized = NormalizeText(command.FullName);
        var baseUsername = string.IsNullOrWhiteSpace(emailLocalPart)
            ? fullNameNormalized
            : NormalizeText(emailLocalPart);

        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            baseUsername = $"user{DateTime.UtcNow:yyyyMMdd}";
        }

        var candidate = baseUsername;
        var suffix = 1;
        while (await _userRepository.ExistsByUsernameAsync(candidate, cancellationToken))
        {
            candidate = $"{baseUsername}{suffix++}";
        }

        return candidate;
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character) || NonAlphaNumericCharacters.Contains(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string ComputeTokenHash(string activationToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(activationToken));
        return Convert.ToHexString(hashBytes);
    }

    private static void Validate(ProvisionUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            throw new ValidationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }
    }
}
