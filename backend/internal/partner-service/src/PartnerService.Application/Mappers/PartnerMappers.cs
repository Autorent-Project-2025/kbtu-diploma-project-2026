using PartnerService.Application.DTOs;
using PartnerService.Domain.Entities;
using System.Linq.Expressions;

namespace PartnerService.Application.Mappers;

public static class PartnerMappers
{
    private static readonly Expression<Func<Partner, PartnerResponseDto>> PartnerProjection = entity => new PartnerResponseDto
    {
        Id = entity.Id,
        OwnerFirstName = entity.OwnerFirstName,
        OwnerLastName = entity.OwnerLastName,
        CreatedOn = entity.CreatedOn,
        ContractFileName = entity.ContractFileName,
        OwnerIdentityFileName = entity.OwnerIdentityFileName,
        RegistrationDate = entity.RegistrationDate,
        PartnershipEndDate = entity.PartnershipEndDate,
        RelatedUserId = entity.RelatedUserId,
        PhoneNumber = entity.PhoneNumber
    };

    public static IQueryable<PartnerResponseDto> SelectToPartnerResponseDto(this IQueryable<Partner> query)
    {
        return query.Select(PartnerProjection);
    }

    public static PartnerResponseDto ToPartnerResponseDto(this Partner entity)
    {
        return new PartnerResponseDto
        {
            Id = entity.Id,
            OwnerFirstName = entity.OwnerFirstName,
            OwnerLastName = entity.OwnerLastName,
            CreatedOn = entity.CreatedOn,
            ContractFileName = entity.ContractFileName,
            OwnerIdentityFileName = entity.OwnerIdentityFileName,
            RegistrationDate = entity.RegistrationDate,
            PartnershipEndDate = entity.PartnershipEndDate,
            RelatedUserId = entity.RelatedUserId,
            PhoneNumber = entity.PhoneNumber
        };
    }
}
