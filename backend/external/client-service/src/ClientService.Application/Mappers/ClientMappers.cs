using ClientService.Application.DTOs;
using ClientService.Domain.Entities;
using System.Linq.Expressions;

namespace ClientService.Application.Mappers;

public static class ClientMappers
{
    private static readonly Expression<Func<Client, ClientResponseDto>> ClientProjection = entity => new ClientResponseDto
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        CreatedOn = entity.CreatedOn,
        BirthDate = entity.BirthDate,
        IdentityDocumentFileName = entity.IdentityDocumentFileName,
        DriverLicenseFileName = entity.DriverLicenseFileName,
        RelatedUserId = entity.RelatedUserId,
        PhoneNumber = entity.PhoneNumber,
        AvatarUrl = entity.AvatarUrl
    };

    public static IQueryable<ClientResponseDto> SelectToClientResponseDto(this IQueryable<Client> query)
    {
        return query.Select(ClientProjection);
    }

    public static ClientResponseDto ToClientResponseDto(this Client entity)
    {
        return new ClientResponseDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            CreatedOn = entity.CreatedOn,
            BirthDate = entity.BirthDate,
            IdentityDocumentFileName = entity.IdentityDocumentFileName,
            DriverLicenseFileName = entity.DriverLicenseFileName,
            RelatedUserId = entity.RelatedUserId,
            PhoneNumber = entity.PhoneNumber,
            AvatarUrl = entity.AvatarUrl
        };
    }
}
