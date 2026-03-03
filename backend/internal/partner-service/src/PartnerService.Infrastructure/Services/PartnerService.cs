using PartnerService.Application.DTOs;
using PartnerService.Application.Interfaces;
using PartnerService.Application.Mappers;
using PartnerService.Domain.Entities;
using PartnerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PartnerService.Infrastructure.Services;

public class PartnerService : IPartnerService
{
    private readonly ApplicationDbContext _db;

    public PartnerService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PartnerResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Partners
            .AsNoTracking()
            .OrderByDescending(partner => partner.CreatedOn)
            .ThenByDescending(partner => partner.Id)
            .SelectToPartnerResponseDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<PartnerResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        return await _db.Partners
            .AsNoTracking()
            .Where(partner => partner.Id == id)
            .SelectToPartnerResponseDto()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PartnerResponseDto?> GetByRelatedUserIdAsync(string relatedUserId, CancellationToken cancellationToken = default)
    {
        var normalizedRelatedUserId = NormalizeRequired(relatedUserId, nameof(relatedUserId), 64);

        return await _db.Partners
            .AsNoTracking()
            .Where(partner => partner.RelatedUserId == normalizedRelatedUserId)
            .SelectToPartnerResponseDto()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PartnerResponseDto> CreateAsync(PartnerCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var normalized = NormalizeAndValidate(
            dto.OwnerFirstName,
            dto.OwnerLastName,
            dto.ContractFileName,
            dto.OwnerIdentityFileName,
            dto.RegistrationDate,
            dto.PartnershipEndDate,
            dto.RelatedUserId,
            dto.PhoneNumber);

        if (await _db.Partners.AnyAsync(partner => partner.RelatedUserId == normalized.RelatedUserId, cancellationToken))
        {
            throw new InvalidOperationException("Partner for this related user already exists.");
        }

        var entity = new Partner
        {
            OwnerFirstName = normalized.OwnerFirstName,
            OwnerLastName = normalized.OwnerLastName,
            CreatedOn = DateTime.UtcNow,
            ContractFileName = normalized.ContractFileName,
            OwnerIdentityFileName = normalized.OwnerIdentityFileName,
            RegistrationDate = normalized.RegistrationDate,
            PartnershipEndDate = normalized.PartnershipEndDate,
            RelatedUserId = normalized.RelatedUserId,
            PhoneNumber = normalized.PhoneNumber
        };

        _db.Partners.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.ToPartnerResponseDto();
    }

    public async Task<PartnerResponseDto?> UpdateAsync(int id, PartnerUpdateDto dto, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);
        ArgumentNullException.ThrowIfNull(dto);

        var normalized = NormalizeAndValidate(
            dto.OwnerFirstName,
            dto.OwnerLastName,
            dto.ContractFileName,
            dto.OwnerIdentityFileName,
            dto.RegistrationDate,
            dto.PartnershipEndDate,
            dto.RelatedUserId,
            dto.PhoneNumber);

        var entity = await _db.Partners.FirstOrDefaultAsync(partner => partner.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var relatedUserExists = await _db.Partners.AnyAsync(
            partner => partner.Id != id && partner.RelatedUserId == normalized.RelatedUserId,
            cancellationToken);

        if (relatedUserExists)
        {
            throw new InvalidOperationException("Partner for this related user already exists.");
        }

        entity.OwnerFirstName = normalized.OwnerFirstName;
        entity.OwnerLastName = normalized.OwnerLastName;
        entity.ContractFileName = normalized.ContractFileName;
        entity.OwnerIdentityFileName = normalized.OwnerIdentityFileName;
        entity.RegistrationDate = normalized.RegistrationDate;
        entity.PartnershipEndDate = normalized.PartnershipEndDate;
        entity.RelatedUserId = normalized.RelatedUserId;
        entity.PhoneNumber = normalized.PhoneNumber;

        await _db.SaveChangesAsync(cancellationToken);

        return entity.ToPartnerResponseDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        var entity = await _db.Partners.FirstOrDefaultAsync(partner => partner.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _db.Partners.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static NormalizedPartnerData NormalizeAndValidate(
        string? ownerFirstName,
        string? ownerLastName,
        string? contractFileName,
        string? ownerIdentityFileName,
        DateOnly registrationDate,
        DateOnly partnershipEndDate,
        string? relatedUserId,
        string? phoneNumber)
    {
        if (registrationDate == default)
        {
            throw new ArgumentException("RegistrationDate is required.", nameof(registrationDate));
        }

        if (partnershipEndDate == default)
        {
            throw new ArgumentException("PartnershipEndDate is required.", nameof(partnershipEndDate));
        }

        if (partnershipEndDate < registrationDate)
        {
            throw new ArgumentException("PartnershipEndDate must be greater than or equal to RegistrationDate.", nameof(partnershipEndDate));
        }

        return new NormalizedPartnerData(
            NormalizeRequired(ownerFirstName, nameof(ownerFirstName), 100),
            NormalizeRequired(ownerLastName, nameof(ownerLastName), 100),
            NormalizeOptional(contractFileName, nameof(contractFileName), 255),
            NormalizeRequired(ownerIdentityFileName, nameof(ownerIdentityFileName), 255),
            registrationDate,
            partnershipEndDate,
            NormalizeRequired(relatedUserId, nameof(relatedUserId), 64),
            NormalizeRequired(phoneNumber, nameof(phoneNumber), 32));
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
            throw new ArgumentException("Partner id must be greater than zero.", nameof(id));
        }
    }

    private sealed record NormalizedPartnerData(
        string OwnerFirstName,
        string OwnerLastName,
        string? ContractFileName,
        string OwnerIdentityFileName,
        DateOnly RegistrationDate,
        DateOnly PartnershipEndDate,
        string RelatedUserId,
        string PhoneNumber);
}
