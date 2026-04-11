using ClientService.Application.DTOs;
using ClientService.Application.Interfaces;
using ClientService.Application.Interfaces.Integrations;
using ClientService.Application.Mappers;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorageClient _imageStorageClient;

    public ClientService(ApplicationDbContext db, IImageStorageClient imageStorageClient)
    {
        _db = db;
        _imageStorageClient = imageStorageClient;
    }

    public async Task<IReadOnlyCollection<ClientResponseDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Client> query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(q) ||
                c.LastName.ToLower().Contains(q) ||
                c.PhoneNumber.ToLower().Contains(q) ||
                c.RelatedUserId.ToLower().Contains(q));
        }

        return await query
            .OrderByDescending(client => client.CreatedOn)
            .ThenByDescending(client => client.Id)
            .SelectToClientResponseDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        return await _db.Clients
            .AsNoTracking()
            .Where(client => client.Id == id)
            .SelectToClientResponseDto()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientResponseDto> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var normalized = NormalizeAndValidate(
            dto.FirstName,
            dto.LastName,
            dto.BirthDate,
            dto.IdentityDocumentFileName,
            dto.DriverLicenseFileName,
            dto.RelatedUserId,
            dto.PhoneNumber,
            dto.AvatarUrl,
            dto.AvatarImageId,
            dto.ProvisionRequestKey);

        if (normalized.ProvisionRequestKey is not null)
        {
            var existingByRequestKey = await _db.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(client => client.ProvisionRequestKey == normalized.ProvisionRequestKey, cancellationToken);

            if (existingByRequestKey is not null)
            {
                EnsureMatchingProvision(existingByRequestKey, normalized);
                return existingByRequestKey.ToClientResponseDto();
            }
        }

        if (await _db.Clients.AnyAsync(client => client.RelatedUserId == normalized.RelatedUserId, cancellationToken))
        {
            throw new InvalidOperationException("Client for this related user already exists.");
        }

        var entity = new Client
        {
            FirstName = normalized.FirstName,
            LastName = normalized.LastName,
            CreatedOn = DateTime.UtcNow,
            BirthDate = normalized.BirthDate,
            IdentityDocumentFileName = normalized.IdentityDocumentFileName,
            DriverLicenseFileName = normalized.DriverLicenseFileName,
            RelatedUserId = normalized.RelatedUserId,
            PhoneNumber = normalized.PhoneNumber,
            AvatarUrl = normalized.AvatarUrl,
            AvatarImageId = normalized.AvatarImageId,
            ProvisionRequestKey = normalized.ProvisionRequestKey
        };

        _db.Clients.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.ToClientResponseDto();
    }

    public async Task<ClientResponseDto?> UpdateAsync(
        int id,
        ClientUpdateDto dto,
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await _db.Clients.FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var normalized = NormalizeAndValidate(
            dto.FirstName,
            dto.LastName,
            dto.BirthDate,
            dto.IdentityDocumentFileName,
            dto.DriverLicenseFileName,
            dto.RelatedUserId,
            dto.PhoneNumber,
            dto.AvatarUrl,
            ResolveAvatarImageId(entity.AvatarUrl, entity.AvatarImageId, dto.AvatarUrl, dto.AvatarImageId),
            null);

        var relatedUserExists = await _db.Clients.AnyAsync(
            client => client.Id != id && client.RelatedUserId == normalized.RelatedUserId,
            cancellationToken);

        if (relatedUserExists)
        {
            throw new InvalidOperationException("Client for this related user already exists.");
        }

        entity.FirstName = normalized.FirstName;
        entity.LastName = normalized.LastName;
        entity.BirthDate = normalized.BirthDate;
        entity.IdentityDocumentFileName = normalized.IdentityDocumentFileName;
        entity.DriverLicenseFileName = normalized.DriverLicenseFileName;
        entity.RelatedUserId = normalized.RelatedUserId;
        entity.PhoneNumber = normalized.PhoneNumber;
        var previousAvatarImageId = entity.AvatarImageId;
        entity.AvatarUrl = normalized.AvatarUrl;
        entity.AvatarImageId = normalized.AvatarImageId;

        await _db.SaveChangesAsync(cancellationToken);
        await TryDeleteReplacedAvatarAsync(
            previousAvatarImageId,
            entity.AvatarImageId,
            authorizationHeader,
            cancellationToken);

        return entity.ToClientResponseDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var entity = await _db.Clients.FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _db.Clients.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ClientBookingAccessDto?> GetBookingAccessByRelatedUserIdAsync(
        string relatedUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedRelatedUserId = NormalizeRequired(relatedUserId, nameof(relatedUserId), 64);

        return await _db.Clients
            .AsNoTracking()
            .Where(client => client.RelatedUserId == normalizedRelatedUserId)
            .Select(client => new ClientBookingAccessDto
            {
                RelatedUserId = client.RelatedUserId,
                BookingActionsBlocked = client.BookingActionsBlocked,
                BookingBlockReason = client.BookingBlockReason,
                BookingBlockedAt = client.BookingBlockedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientResponseDto?> SetBookingActionsBlockedByRelatedUserIdAsync(
        string relatedUserId,
        bool isBlocked,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var normalizedRelatedUserId = NormalizeRequired(relatedUserId, nameof(relatedUserId), 64);
        var normalizedReason = NormalizeOptional(reason, nameof(reason), 512);

        var entity = await _db.Clients.FirstOrDefaultAsync(
            client => client.RelatedUserId == normalizedRelatedUserId,
            cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.BookingActionsBlocked = isBlocked;
        entity.BookingBlockReason = isBlocked ? normalizedReason : null;
        entity.BookingBlockedAt = isBlocked ? DateTimeOffset.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);
        return entity.ToClientResponseDto();
    }

    private static NormalizedClientData NormalizeAndValidate(
        string? firstName,
        string? lastName,
        DateOnly birthDate,
        string? identityDocumentFileName,
        string? driverLicenseFileName,
        string? relatedUserId,
        string? phoneNumber,
        string? avatarUrl,
        string? avatarImageId,
        string? provisionRequestKey)
    {
        if (birthDate == default)
        {
            throw new ArgumentException("BirthDate is required.", nameof(birthDate));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(birthDate));
        }

        var normalizedAvatar = NormalizeAvatar(avatarUrl, avatarImageId);

        return new NormalizedClientData(
            NormalizeRequired(firstName, nameof(firstName), 100),
            NormalizeRequired(lastName, nameof(lastName), 100),
            birthDate,
            NormalizeOptional(identityDocumentFileName, nameof(identityDocumentFileName), 255),
            NormalizeOptional(driverLicenseFileName, nameof(driverLicenseFileName), 255),
            NormalizeRequired(relatedUserId, nameof(relatedUserId), 64),
            NormalizeRequired(phoneNumber, nameof(phoneNumber), 32),
            normalizedAvatar.AvatarUrl,
            normalizedAvatar.AvatarImageId,
            NormalizeOptional(provisionRequestKey, nameof(provisionRequestKey), 128));
    }

    private static NormalizedAvatarData NormalizeAvatar(string? avatarUrl, string? avatarImageId)
    {
        var normalizedAvatarUrl = NormalizeOptional(avatarUrl, nameof(avatarUrl), 1024);
        if (normalizedAvatarUrl is not null &&
            !Uri.TryCreate(normalizedAvatarUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("AvatarUrl must be a valid absolute URL.", nameof(avatarUrl));
        }

        var normalizedAvatarImageId = NormalizeOptional(avatarImageId, nameof(avatarImageId), 255);
        if (normalizedAvatarImageId is not null)
        {
            if (normalizedAvatarUrl is null)
            {
                throw new ArgumentException("AvatarImageId requires AvatarUrl.", nameof(avatarImageId));
            }

            if (!AvatarUrlMatchesImageId(normalizedAvatarUrl, normalizedAvatarImageId))
            {
                throw new ArgumentException(
                    "AvatarImageId must match the file name in AvatarUrl.",
                    nameof(avatarImageId));
            }
        }

        return new NormalizedAvatarData(normalizedAvatarUrl, normalizedAvatarImageId);
    }

    private static bool AvatarUrlMatchesImageId(string avatarUrl, string avatarImageId)
    {
        if (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out var avatarUri))
        {
            return false;
        }

        var fileName = Path.GetFileName(Uri.UnescapeDataString(avatarUri.AbsolutePath));
        return string.Equals(fileName, avatarImageId, StringComparison.Ordinal);
    }

    private static string? ResolveAvatarImageId(
        string? currentAvatarUrl,
        string? currentAvatarImageId,
        string? requestedAvatarUrl,
        string? requestedAvatarImageId)
    {
        if (string.IsNullOrWhiteSpace(requestedAvatarUrl))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(requestedAvatarImageId))
        {
            return requestedAvatarImageId;
        }

        return string.Equals(currentAvatarUrl, requestedAvatarUrl, StringComparison.Ordinal)
            ? currentAvatarImageId
            : null;
    }

    private async Task TryDeleteReplacedAvatarAsync(
        string? previousAvatarImageId,
        string? currentAvatarImageId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(previousAvatarImageId) ||
            string.Equals(previousAvatarImageId, currentAvatarImageId, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return;
        }

        try
        {
            await _imageStorageClient.DeleteAsync(
                previousAvatarImageId,
                authorizationHeader,
                cancellationToken);
        }
        catch
        {
            // Profile update is already persisted; failure here should not roll it back.
        }
    }

    private static string NormalizeRequired(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);
        }

        return normalized;
    }

    private static void EnsureValidId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Client id must be greater than zero.", nameof(id));
        }
    }

    private sealed record NormalizedClientData(
        string FirstName,
        string LastName,
        DateOnly BirthDate,
        string? IdentityDocumentFileName,
        string? DriverLicenseFileName,
        string RelatedUserId,
        string PhoneNumber,
        string? AvatarUrl,
        string? AvatarImageId,
        string? ProvisionRequestKey);

    private sealed record NormalizedAvatarData(
        string? AvatarUrl,
        string? AvatarImageId);

    private static void EnsureMatchingProvision(Client existingClient, NormalizedClientData normalized)
    {
        if (!string.Equals(existingClient.FirstName, normalized.FirstName, StringComparison.Ordinal) ||
            !string.Equals(existingClient.LastName, normalized.LastName, StringComparison.Ordinal) ||
            existingClient.BirthDate != normalized.BirthDate ||
            !string.Equals(existingClient.IdentityDocumentFileName, normalized.IdentityDocumentFileName, StringComparison.Ordinal) ||
            !string.Equals(existingClient.DriverLicenseFileName, normalized.DriverLicenseFileName, StringComparison.Ordinal) ||
            !string.Equals(existingClient.RelatedUserId, normalized.RelatedUserId, StringComparison.Ordinal) ||
            !string.Equals(existingClient.PhoneNumber, normalized.PhoneNumber, StringComparison.Ordinal) ||
            !string.Equals(existingClient.AvatarUrl, normalized.AvatarUrl, StringComparison.Ordinal) ||
            !string.Equals(existingClient.AvatarImageId, normalized.AvatarImageId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Provision request key is already used for another client payload.");
        }
    }

        
    public async Task<ClientResponseDto?> GetByRelatedUserIdAsync(
        string relatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relatedUserId))
        {
            throw new ArgumentException("RelatedUserId is required.", nameof(relatedUserId));
        }

        return await _db.Clients
            .AsNoTracking()
            .Where(client => client.RelatedUserId == relatedUserId)
            .SelectToClientResponseDto()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientResponseDto?> UpdateByRelatedUserIdAsync(
        string relatedUserId,
        ProfileUpdateDto dto,
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relatedUserId))
        {
            throw new ArgumentException("RelatedUserId is required.", nameof(relatedUserId));
        }

        ArgumentNullException.ThrowIfNull(dto);

        var entity = await _db.Clients
            .FirstOrDefaultAsync(client => client.RelatedUserId == relatedUserId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (dto.BirthDate == default)
        {
            throw new ArgumentException("BirthDate is required.", nameof(dto.BirthDate));
        }

        if (dto.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(dto.BirthDate));
        }

        entity.FirstName = NormalizeRequired(dto.FirstName, nameof(dto.FirstName), 100);
        entity.LastName = NormalizeRequired(dto.LastName, nameof(dto.LastName), 100);
        entity.BirthDate = dto.BirthDate;
        entity.PhoneNumber = NormalizeRequired(dto.PhoneNumber, nameof(dto.PhoneNumber), 32);

        var normalizedAvatar = NormalizeAvatar(
            dto.AvatarUrl,
            ResolveAvatarImageId(
                entity.AvatarUrl,
                entity.AvatarImageId,
                dto.AvatarUrl,
                dto.AvatarImageId));
        var previousAvatarImageId = entity.AvatarImageId;
        entity.AvatarUrl = normalizedAvatar.AvatarUrl;
        entity.AvatarImageId = normalizedAvatar.AvatarImageId;

        await _db.SaveChangesAsync(cancellationToken);
        await TryDeleteReplacedAvatarAsync(
            previousAvatarImageId,
            entity.AvatarImageId,
            authorizationHeader,
            cancellationToken);

        return entity.ToClientResponseDto();
    }

}
