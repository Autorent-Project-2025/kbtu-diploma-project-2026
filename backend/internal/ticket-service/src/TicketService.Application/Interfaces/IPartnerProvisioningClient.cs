using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IPartnerProvisioningClient
{
    Task ProvisionPartnerAsync(
        ProvisionPartnerProfileRequest request,
        CancellationToken cancellationToken = default);
}
