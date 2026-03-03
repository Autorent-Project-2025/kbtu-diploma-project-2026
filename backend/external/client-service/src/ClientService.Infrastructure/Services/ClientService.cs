using ClientService.Application.DTOs;
using ClientService.Application.Interfaces;
using ClientService.Application.Mappers;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _db;

    public ClientService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<ClientResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Clients
            .AsNoTracking()
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
            dto.AvatarUrl);

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
            AvatarUrl = normalized.AvatarUrl
        };

        _db.Clients.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.ToClientResponseDto();
    }

    public async Task<ClientResponseDto?> UpdateAsync(int id, ClientUpdateDto dto, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);
        ArgumentNullException.ThrowIfNull(dto);

        var normalized = NormalizeAndValidate(
            dto.FirstName,
            dto.LastName,
            dto.BirthDate,
            dto.IdentityDocumentFileName,
            dto.DriverLicenseFileName,
            dto.RelatedUserId,
            dto.PhoneNumber,
            dto.AvatarUrl);

        var entity = await _db.Clients.FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

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
        entity.AvatarUrl = normalized.AvatarUrl;

        await _db.SaveChangesAsync(cancellationToken);

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

    private static NormalizedClientData NormalizeAndValidate(
        string? firstName,
        string? lastName,
        DateOnly birthDate,
        string? identityDocumentFileName,
        string? driverLicenseFileName,
        string? relatedUserId,
        string? phoneNumber,
        string? avatarUrl)
    {
        if (birthDate == default)
        {
            throw new ArgumentException("BirthDate is required.", nameof(birthDate));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(birthDate));
        }

        var normalizedAvatarUrl = NormalizeOptional(avatarUrl, nameof(avatarUrl), 1024);
        if (normalizedAvatarUrl is not null &&
            !Uri.TryCreate(normalizedAvatarUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("AvatarUrl must be a valid absolute URL.", nameof(avatarUrl));
        }

        return new NormalizedClientData(
            NormalizeRequired(firstName, nameof(firstName), 100),
            NormalizeRequired(lastName, nameof(lastName), 100),
            birthDate,
            NormalizeOptional(identityDocumentFileName, nameof(identityDocumentFileName), 255),
            NormalizeOptional(driverLicenseFileName, nameof(driverLicenseFileName), 255),
            NormalizeRequired(relatedUserId, nameof(relatedUserId), 64),
            NormalizeRequired(phoneNumber, nameof(phoneNumber), 32),
            normalizedAvatarUrl);
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
        string? AvatarUrl);
}
