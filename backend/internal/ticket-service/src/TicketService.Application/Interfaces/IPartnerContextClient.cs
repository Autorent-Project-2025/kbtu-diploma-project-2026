using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IPartnerContextClient
{
    Task<PartnerContextResult?> GetCurrentPartnerAsync(
        string authorizationHeader,
        CancellationToken cancellationToken = default);
}
