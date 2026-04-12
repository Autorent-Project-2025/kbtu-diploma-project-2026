namespace PartnerService.Application.Interfaces;

public interface ICarServiceClient
{
    Task<int> SetPartnerCarsActiveAsync(Guid partnerUserId, bool isActive, CancellationToken cancellationToken = default);
}
