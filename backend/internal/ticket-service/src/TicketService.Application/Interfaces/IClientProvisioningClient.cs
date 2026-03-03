using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IClientProvisioningClient
{
    Task ProvisionClientAsync(
        ProvisionClientProfileRequest request,
        CancellationToken cancellationToken = default);
}
