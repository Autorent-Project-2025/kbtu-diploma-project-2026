using IdentityService.Application.Constants;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Utils;
using IdentityService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Commands.ProvisionUser;

public sealed class ProvisionUserCommandHandler
{
    private static readonly char[] NonAlphaNumericCharacters = ['-', '_', '.'];

    private readonly IUserRepository _userRepository;
    private readonly IUserProvisionRequestRepository _userProvisionRequestRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ProvisionUserCommandHandler(
        IUserRepository userRepository,
        IUserProvisionRequestRepository userProvisionRequestRepository,
        IRoleRepository roleRepository,
        IActivationTokenRepository activationTokenRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userProvisionRequestRepository = userProvisionRequestRepository;
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

        var normalizedRequestKey = NormalizeRequestKey(command.RequestKey);
        if (normalizedRequestKey is not null)
        {
            var existingRequest = await _userProvisionRequestRepository.GetByRequestKeyAsync(normalizedRequestKey, cancellationToken);
            if (existingRequest is not null)
            {
                EnsureMatchingRequest(existingRequest, command);

                var existingUser = await _userRepository.GetByIdAsync(existingRequest.UserId, cancellationToken: cancellationToken);
                if (existingUser is null)
                {
                    throw new NotFoundException($"Provisioned user '{existingRequest.UserId}' was not found.");
                }

                var repeatedActivation = await CreateActivationTokenAsync(existingUser.Id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new ProvisionUserResult(
                    existingUser.Id,
                    existingUser.Username,
                    existingUser.Email,
                    repeatedActivation.ActivationToken,
                    repeatedActivation.ActivationExpiresAtUtc);
            }
        }

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
        var user = new User(
            Guid.NewGuid(),
            username,
            normalizedEmail,
            _passwordHasher.Hash(temporaryPassword),
            subjectType: UserTypeValidator.ValidateSubjectType(command.SubjectType),
            actorType: UserTypeValidator.ValidateActorType(command.ActorType));
        user.AssignRole(userRole);

        await _userRepository.AddAsync(user, cancellationToken);
        if (normalizedRequestKey is not null)
        {
            await _userProvisionRequestRepository.AddAsync(
                new UserProvisionRequest
                {
                    Id = Guid.NewGuid(),
                    RequestKey = normalizedRequestKey,
                    UserId = user.Id,
                    FullName = NormalizeComparableText(command.FullName),
                    Email = normalizedEmail,
                    BirthDate = command.BirthDate,
                    SubjectType = NormalizeComparableText(command.SubjectType),
                    ActorType = NormalizeComparableText(command.ActorType),
                    CreatedAtUtc = DateTime.UtcNow
                },
                cancellationToken);
        }

        var activation = await CreateActivationTokenAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProvisionUserResult(
            user.Id,
            user.Username,
            user.Email,
            activation.ActivationToken,
            activation.ActivationExpiresAtUtc);
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

    private async Task<(string ActivationToken, DateTime ActivationExpiresAtUtc)> CreateActivationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var activationToken = GenerateToken();
        var activationTokenHash = ComputeTokenHash(activationToken);
        var activationExpiresAtUtc = nowUtc.AddHours(24);

        var activationTokenEntity = new ActivationToken(
            Guid.NewGuid(),
            userId,
            activationTokenHash,
            nowUtc,
            activationExpiresAtUtc);

        await _activationTokenRepository.AddAsync(activationTokenEntity, cancellationToken);
        return (activationToken, activationExpiresAtUtc);
    }

    private static string? NormalizeRequestKey(string? requestKey)
    {
        if (string.IsNullOrWhiteSpace(requestKey))
        {
            return null;
        }

        var normalized = requestKey.Trim();
        if (normalized.Length > 128)
        {
            throw new ValidationException("Request key length must not exceed 128.");
        }

        return normalized;
    }

    private static string NormalizeComparableText(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static void EnsureMatchingRequest(UserProvisionRequest existingRequest, ProvisionUserCommand command)
    {
        var normalizedEmail = NormalizeComparableText(command.Email);
        var normalizedFullName = NormalizeComparableText(command.FullName);
        var normalizedSubjectType = NormalizeComparableText(command.SubjectType);
        var normalizedActorType = NormalizeComparableText(command.ActorType);

        if (!string.Equals(existingRequest.Email, normalizedEmail, StringComparison.Ordinal) ||
            !string.Equals(existingRequest.FullName, normalizedFullName, StringComparison.Ordinal) ||
            existingRequest.BirthDate != command.BirthDate ||
            !string.Equals(existingRequest.SubjectType, normalizedSubjectType, StringComparison.Ordinal) ||
            !string.Equals(existingRequest.ActorType, normalizedActorType, StringComparison.Ordinal))
        {
            throw new ConflictException("Request key is already used for another user provisioning payload.");
        }
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
