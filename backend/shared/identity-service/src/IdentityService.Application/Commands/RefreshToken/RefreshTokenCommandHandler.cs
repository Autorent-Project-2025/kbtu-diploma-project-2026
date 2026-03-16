using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Application.Utils;
using IdentityService.Domain.Entities;
using RefreshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthTokensResult> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        var nowUtc = DateTime.UtcNow;
        var refreshTokenHash = _jwtProvider.ComputeRefreshTokenHash(command.RefreshToken.Trim());

        var storedRefreshToken = await _refreshTokenRepository.GetByTokenHashWithUserAsync(
            refreshTokenHash,
            cancellationToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive(nowUtc))
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        var user = storedRefreshToken.User;
        if (user is null)
        {
            throw new UnauthorizedException("Refresh token owner was not found.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is deactivated.");
        }

        storedRefreshToken.Revoke(nowUtc);

        var roles = await _roleRepository.ListAsync(
            includePermissions: true,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        var roleGraph = RolePermissionResolver.BuildGraph(roles);
        var permissions = RolePermissionResolver.ResolveEffectivePermissions(
            user.Roles.Select(role => role.Id),
            roleGraph);

        var accessToken = _jwtProvider.GenerateAccessToken(
            user.Id,
            user.Username,
            user.SubjectType,
            user.ActorType,
            permissions);
        var rotatedRefreshToken = _jwtProvider.GenerateRefreshToken();

        var newRefreshToken = new RefreshTokenEntity(
            Guid.NewGuid(),
            user.Id,
            _jwtProvider.ComputeRefreshTokenHash(rotatedRefreshToken.Token),
            nowUtc,
            rotatedRefreshToken.ExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            rotatedRefreshToken.Token,
            rotatedRefreshToken.ExpiresAtUtc);
    }
}
